import { Request, Response } from 'express';
import { prisma } from '../config/db.js';
import getNextSequence from '../helpers/invoiceWorkOrderGenerator';

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

    const line = await prisma.workOrderLine.create({
      data: {
        workOrderId,
        productId,
        quantity,
        price,
        discount: discount || 0
      }
    });

    res.status(201).json(line);
  } catch (error) {
    res.status(500).json({ error: "Couldn't add line to work order" });
  }
};

export const convertToInvoice = async (req: Request, res: Response) => {
  const { workOrderId } = req.params;

  try {
    const wo = await prisma.workOrder.findUnique({
      where: { id: Number(workOrderId) },
      include: {
        lines: { include: { product: true } },
        vehicle: { include: { customer: true } }
      }
    });

    if (!wo) return res.status(404).json({ message: "Work order not found" });

    const invoiceNumber = await getNextSequence(prisma,'F');

    let subtotal = 0;
    const lines = wo.lines.map(line => {
      const lineTotal = Number(line.price) * line.quantity;
      subtotal += lineTotal;
      return {
        description: line.product.name,
        quantity: line.quantity,
        unitPrice: line.price,
        taxRate: line.product.taxRate,
        total: lineTotal,
        productId: line.productId
      };
    });

    const taxTotal = subtotal * 0.21;
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
        data: { status: 'COMPLETED' }
      });

      return inv;
    });

    res.status(201).json(invoice);
  } catch (error) {
    res.status(500).json({ error: "Error generating invoice", details: error });
  }
};