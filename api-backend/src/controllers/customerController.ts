import { Request, Response } from 'express';
import { prisma } from '../config/db.js';

//create customer
export const createCustomer = async (req: Request, res: Response) => {
  const { nif, name, address, phonenumber, email } = req.body;
  
  try {
    const newCustomer = await prisma.customer.create({
      data: {
        nif,
        name,
        address,
        phonenumber,
        email
      },
    });
    return res.status(201).json(newCustomer);
  } catch (error: any) {
    if (error.code === 'P2002') {
      return res.status(400).json({ message: "Nif or email already in use" });
    }
    res.status(500).json({ message: "Error on creation", error: error.message });
  }
};

//get
export const getCustomerById = async (req: Request, res: Response) => {
  const { id } = req.params;
  try {
    const customer = await prisma.customer.findUnique({
      where: { id: parseInt(id) },
      include: { vehicles: true }
    });

    if (!customer) return res.status(404).json({ message: "customer not found" });
    
    return res.status(200).json(customer);
  } catch (error: any) {
    res.status(500).json({ message: "Error obtaining customer", error: error.message });
  }
};

export const getCustomers = async (req: Request, res: Response) => {
  try {
    const customers = await prisma.customer.findMany({
      // include: { vehicles: true } 
    });

    return res.status(200).json(customers);
  } catch (error: any) {
    res.status(500).json({ 
      message: "Error obtaining customers", 
      error: error.message 
    });
  }
};