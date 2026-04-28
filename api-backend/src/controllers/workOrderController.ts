import { Request, Response } from 'express';
import { prisma } from '../config/db.js';
import getNextSequence from '../helpers/invoiceWorkOrderGenerator';
 
//work orders
export const createWorkOrder = async (req: Request, res: Response) => {
  try {
    const { vehicleId, description } = req.body;
 
    const orderNumber = await getNextSequence(prisma,'W');
 
    const newOrder = await prisma.workOrder.create({
      data: {
        orderNumber,
        description,
        vehicleId,
        status: 'PENDING'
      }
    });
 
    res.status(201).json(newOrder);
  } catch (error: any) {
  res.status(500).json({ 
    error: "Error generating work order", 
    message: error.message,
    code: error.code
  });
  }
};
 
export const addLineToWorkOrder = async (req: Request, res: Response) => {
  try {
    const { workOrderId, productId, quantity, price, discount } = req.body;
 
    const product = await prisma.product.findUnique({
      where: { id: productId },
      select: { id: true, stock: true, name: true, type: true }
    });
 
    if (!product) {
      return res.status(404).json({ error: "Product not found" });
    }
 
    const isService = product.type === 'SERVICE';
 
    if (!isService && product.stock < quantity) {
      return res.status(409).json({ 
        error: "Insufficient stock", 
        available: product.stock,
        requested: quantity
      });
    }
 
    const operations: any[] = [
      prisma.workOrderLine.create({
        data: {
          workOrderId,
          productId,
          quantity,
          price,
          discount: discount || 0
        }
      })
    ];
 
    if (!isService) {
      operations.push(
        prisma.product.update({
          where: { id: productId },
          data: { stock: { decrement: quantity } }
        })
      );
    }
 
    const [line] = await prisma.$transaction(operations);
 
    res.status(201).json(line);
  } catch (error: any) {
    res.status(500).json({ error: "Couldn't add line to work order", message: error.message });
  }
};
 
//general get
export const getWorkOrders = async (req: Request, res: Response) => {
  try {
    const orders = await prisma.workOrder.findMany({
      include: {
        vehicle: {
          select: {
            plate: true,
            brand: true,
            model: true,
            customer: {
              select: {
                name: true
              }
            }
          }
        },
        lines: true,
        _count: {
          select: { lines: true }
        }
      },
      orderBy: {
        createdAt: 'desc'
      }
    });
 
    res.status(200).json(orders);
  } catch (error: any) {
    res.status(500).json({ 
      error: "Error fetching Work orders", 
      message: error.message 
    });
  }
};
 
//for detailed work order
export const getWorkOrderById = async (req: Request, res: Response) => {
  const { id } = req.params;
 
  try {
    const order = await prisma.workOrder.findUnique({
      where: { id: Number(id) },
      include: {
        vehicle: {
          include: { customer: true }
        },
        lines: {
          include: {
            product: {
              select: { name: true, sku: true}
            }
          }
        },
        invoice: {
          select: { invoiceNumber: true, total: true }
        }
      }
    });
 
    if (!order) {
      return res.status(404).json({ message: "Nonexistant work order" });
    }
 
    res.status(200).json(order);
  } catch (error: any) {
    res.status(500).json({ error: "Error on search", message: error.message });
  }
};
 
//invoices
export const convertToInvoice = async (req: Request, res: Response) => {
  const { id: workOrderId } = req.params;
 
  try {
    const wo = await prisma.workOrder.findUnique({
      where: { id: Number(workOrderId) },
      include: {
        lines: { include: { product: true } },
        vehicle: { include: { customer: true } },
        invoice: { select: { id: true, invoiceNumber: true } }
      }
    });
 
    if (!wo) {
      return res.status(404).json({ message: "Work order not found" });
    }
 
    if (wo.status === 'CANCELLED') {
      return res.status(400).json({ message: "Cannot invoice a cancelled work order" });
    }
 
    if (wo.invoice) {
      return res.status(409).json({ 
        message: "This work order already has an invoice", 
        invoiceNumber: wo.invoice.invoiceNumber 
      });
    }
 
    if (wo.lines.length === 0) {
      return res.status(400).json({ message: "Cannot invoice a work order with no lines" });
    }
 
    const invoiceNumber = await getNextSequence(prisma, 'F');
    
    let subtotal = 0;
    let taxTotal = 0;
 
    const lines = wo.lines.map(line => {
      const lineSubtotal = Number(line.price) * line.quantity;
      const lineTax = lineSubtotal * (Number(line.product.taxRate) / 100);
      subtotal += lineSubtotal;
      taxTotal += lineTax;
      return {
        description: line.product.name,
        quantity: line.quantity,
        unitPrice: line.price,
        taxRate: line.product.taxRate,
        total: lineSubtotal + lineTax,
        productId: line.productId
      };
    });
 
    const total = subtotal + taxTotal;
 
    const invoice = await prisma.$transaction(async (tx) => {
      const inv = await tx.invoice.create({
        data: {
          invoiceNumber,
          customerId: wo.vehicle.customerId,
          workOrderId: wo.id,
          subtotal,
          taxTotal,
          total,
          status: 'ISSUED',
          lines: { create: lines }
        }
      });
 
      await tx.workOrder.update({
        where: { id: wo.id },
        data: { 
          status: 'COMPLETED',
          completedAt: new Date()
        }
      });
 
      return inv;
    });
 
    res.status(201).json(invoice);
  } catch (error: any) {
    res.status(500).json({ error: "Error generating invoice", message: error.message });
  }
};
 
export const updateWorkOrder = async (req: Request, res: Response) => {
  const { id } = req.params;
  const { description, status, vehicleId } = req.body;
 
  try {
    const updatedOrder = await prisma.workOrder.update({
      where: { id: Number(id) },
      data: {
        description,
        status,
        vehicleId
      }
    });
    res.status(200).json(updatedOrder);
  } catch (error: any) {
    res.status(500).json({ error: "Update failed", message: error.message });
  }
};

router.get('/getInvoices', async (req, res) => {
  try {
    const invoices = await prisma.invoice.findMany({
      include: {
        lines: {
          include: {
            product: true
          }
        },
        customer: true,
        workOrder: true
      },
      orderBy: {
        createdAt: 'desc'
      }
    });

    return res.status(200).json(invoices);
  } catch (error) {
    console.error('Error fetching invoices:', error);
    return res.status(500).json({ message: 'Internal server error' });
  }
});