import { Request, Response } from 'express';
import { prisma } from '../config/db.js';

export const getAppointments = async (req: Request, res: Response) => {
  try {
    const appointments = await prisma.appointment.findMany({
      include: {
        vehicle: {
          include: { customer: true }
        }
      },
      orderBy: {
        scheduledAt: 'asc'
      }
    });
    res.status(200).json(appointments);
  } catch (error: any) {
    res.status(500).json({ 
      error: "Error fetching appointments", 
      message: error.message 
    });
  }
};

export const createAppointment = async (req: Request, res: Response) => {
  try {
    const { vehicleId, customerId, scheduledAt, description } = req.body;

    if (!vehicleId || !customerId) {
      return res.status(400).json({ error: "Missing vehicleId or customerId" });
    }

    const newAppointment = await prisma.appointment.create({
      data: {
        scheduledAt: new Date(scheduledAt),
        description,
        status: 'SCHEDULED',
        vehicle: { connect: { id: Number(vehicleId) } },
        customer: { connect: { id: Number(customerId) } }
      }
    });

    res.status(201).json(newAppointment);
  } catch (error: any) {
    res.status(500).json({ 
      error: "Error creating appointment", 
      message: error.message 
    });
  }
};