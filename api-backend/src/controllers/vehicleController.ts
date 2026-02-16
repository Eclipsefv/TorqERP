import { Request, Response } from 'express';
import { prisma } from '../config/db.js';

export const insertVehicle = async (req: Request, res: Response) => {

  const { plate, brand, model, year, customerId } = req.body;

  try {
    const newVehicle = await prisma.vehicle.create({
      data: {
        plate,
        brand,
        model,
        year,
        customerId,
      },
    });
    
    return res.status(201).json(newVehicle);

  } catch (error: any) {
    if (error.code === 'P2002') {
      return res.status(400).json({ 
        message: "Plate already on db" 
      });
    }

    if (error.code === 'P2003') {
      return res.status(400).json({ 
        message: "Nonexistant customer" 
      });
    }

    return res.status(500).json({ 
      message: "Error creating vehicle", 
      error: error.message 
    });
  }
};