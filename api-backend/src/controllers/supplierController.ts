import { Request, Response } from 'express';
import { prisma } from '../config/db.js';
import getNextSequence from '../helpers/invoiceWorkOrderGenerator.js';
 
export const createSupplier = async (req: Request, res: Response) => {
  try {
    const { cif, name, address, phone, email, notes } = req.body;
 
    if (!cif || !name) {
      return res.status(400).json({ message: 'CIF and name are required' });
    }
 
    const existing = await prisma.supplier.findFirst({
      where: { cif }
    });
 
    if (existing) {
      return res.status(409).json({ message: 'A supplier with this CIF already exists' });
    }
 
    const newSupplier = await prisma.supplier.create({
      data: {
        cif,
        name,
        address: address || null,
        phone:   phone   || null,
        email:   email   || null,
        notes:   notes   || null
      }
    });
 
    res.status(201).json(newSupplier);
  } catch (error: any) {
    res.status(500).json({
      error: 'Error creating supplier',
      message: error.message,
      code: error.code
    });
  }
};
 
export const getSuppliers = async (req: Request, res: Response) => {
  try {
    const suppliers = await prisma.supplier.findMany({
      include: {
        _count: { select: { deliveryNotes: true } }
      },
      orderBy: { createdAt: 'desc' }
    });
 
    res.status(200).json(suppliers);
  } catch (error: any) {
    res.status(500).json({ error: 'Error fetching suppliers', message: error.message });
  }
};
 
export const getSupplierById = async (req: Request, res: Response) => {
  const { id } = req.params;
 
  try {
    const supplier = await prisma.supplier.findUnique({
      where: { id: Number(id) },
      include: {
        deliveryNotes: {
          orderBy: { createdAt: 'desc' },
          include: { _count: { select: { lines: true } } }
        }
      }
    });
 
    if (!supplier) {
      return res.status(404).json({ message: 'Supplier not found' });
    }
 
    res.status(200).json(supplier);
  } catch (error: any) {
    res.status(500).json({ error: 'Error fetching supplier', message: error.message });
  }
};
 
export const updateSupplier = async (req: Request, res: Response) => {
  const { id } = req.params;
  const { cif, name, address, phone, email, notes, active } = req.body;
 
  try {
    const updated = await prisma.supplier.update({
      where: { id: Number(id) },
      data: {
        cif,
        name,
        address: address || null,
        phone:   phone   || null,
        email:   email   || null,
        notes:   notes   || null,
        active
      }
    });
 
    res.status(200).json(updated);
  } catch (error: any) {
    if (error.code === 'P2025') {
      return res.status(404).json({ message: 'Supplier not found' });
    }
    if (error.code === 'P2002') {
      return res.status(409).json({ message: 'CIF or email already in use' });
    }
    res.status(500).json({ error: 'Update failed', message: error.message });
  }
};
 
export const createDeliveryNote = async (req: Request, res: Response) => {
  try {
    const { supplierId, supplierNoteNumber, date, notes } = req.body;
 
    if (!supplierId || !supplierNoteNumber) {
      return res.status(400).json({ message: 'supplierId and supplierNoteNumber are required' });
    }
 
    const supplier = await prisma.supplier.findUnique({
      where: { id: Number(supplierId) }
    });
 
    if (!supplier) {
      return res.status(404).json({ message: 'Supplier not found' });
    }
 
    const internalNumber = await getNextSequence(prisma, 'ALB');
 
    const newNote = await prisma.deliveryNote.create({
      data: {
        internalNumber,
        supplierNoteNumber,
        supplierId: Number(supplierId),
        Date:  date ? new Date(date) : new Date(),
        notes: notes || null,
        subtotal: 0,
        taxTotal: 0,
        total:    0
      }
    });
 
    res.status(201).json(newNote);
  } catch (error: any) {
    res.status(500).json({
      error: 'Error creating delivery note',
      message: error.message,
      code: error.code
    });
  }
};
 
export const addLineToDeliveryNote = async (req: Request, res: Response) => {
  try {
    const { deliveryNoteId, productId, quantity, unitCost, taxRate, discount } = req.body;
 
    if (!deliveryNoteId || !productId || !quantity || unitCost == null) {
      return res.status(400).json({ message: 'deliveryNoteId, productId, quantity and unitCost are required' });
    }
 
    const product = await prisma.product.findUnique({
      where: { id: Number(productId) },
      select: { id: true, taxRate: true, type: true }
    });
 
    if (!product) {
      return res.status(404).json({ message: 'Product not found' });
    }
 
    const effectiveTaxRate  = taxRate  != null ? Number(taxRate)  : Number(product.taxRate);
    const effectiveDiscount = discount != null ? Number(discount) : 0;
    const base       = Number(unitCost) * Number(quantity);
    const discounted = base - (base * effectiveDiscount / 100);
    const tax        = discounted * (effectiveTaxRate / 100);
    const lineTotal  = discounted + tax;
 
    const operations: any[] = [
      prisma.deliveryNoteLine.create({
        data: {
          deliveryNoteId: Number(deliveryNoteId),
          productId:      Number(productId),
          quantity:       Number(quantity),
          unitCost:       Number(unitCost),
          taxRate:        effectiveTaxRate,
          discount:       effectiveDiscount,
          lineTotal
        }
      })
    ];
 
    if (product.type === 'ITEM') {
      operations.push(
        prisma.product.update({
          where: { id: Number(productId) },
          data: {
            stock:    { increment: Number(quantity) },
            buyPrice: Number(unitCost)
          }
        })
      );
    }
 
    const [line] = await prisma.$transaction(operations);
 
    await recalculateNoteTotals(Number(deliveryNoteId));
 
    res.status(201).json(line);
  } catch (error: any) {
    res.status(500).json({ error: 'Error adding line', message: error.message });
  }
};
 
export const getDeliveryNotes = async (req: Request, res: Response) => {
  try {
    const notes = await prisma.deliveryNote.findMany({
      include: {
        supplier: { select: { name: true, cif: true } },
        _count:   { select: { lines: true } }
      },
      orderBy: { createdAt: 'desc' }
    });
 
    res.status(200).json(notes);
  } catch (error: any) {
    res.status(500).json({ error: 'Error fetching delivery notes', message: error.message });
  }
};
 
export const getDeliveryNoteById = async (req: Request, res: Response) => {
  const { id } = req.params;
 
  try {
    const note = await prisma.deliveryNote.findUnique({
      where: { id: Number(id) },
      include: {
        supplier: true,
        lines: {
          include: {
            product: { select: { name: true, sku: true, buyPrice: true } }
          }
        }
      }
    });
 
    if (!note) {
      return res.status(404).json({ message: 'Delivery note not found' });
    }
 
    res.status(200).json(note);
  } catch (error: any) {
    res.status(500).json({ error: 'Error fetching delivery note', message: error.message });
  }
};
 
export const updateDeliveryNote = async (req: Request, res: Response) => {
  const { id } = req.params;
  const { supplierNoteNumber, date, notes } = req.body;
 
  try {
    const updated = await prisma.deliveryNote.update({
      where: { id: Number(id) },
      data: {
        supplierNoteNumber,
        Date:  date ? new Date(date) : undefined,
        notes: notes || null
      }
    });
 
    res.status(200).json(updated);
  } catch (error: any) {
    if (error.code === 'P2025') {
      return res.status(404).json({ message: 'Delivery note not found' });
    }
    res.status(500).json({ error: 'Update failed', message: error.message });
  }
};

async function recalculateNoteTotals(deliveryNoteId: number) {
  const lines = await prisma.deliveryNoteLine.findMany({
    where: { deliveryNoteId }
  });
 
  let subtotal = 0;
  let taxTotal = 0;
 
  for (const line of lines) {
    const base       = Number(line.unitCost) * Number(line.quantity);
    const discounted = base - (base * Number(line.discount) / 100);
    const tax        = discounted * (Number(line.taxRate) / 100);
    subtotal += discounted;
    taxTotal += tax;
  }
 
  await prisma.deliveryNote.update({
    where: { id: deliveryNoteId },
    data:  { subtotal, taxTotal, total: subtotal + taxTotal }
  });
}
 