import { Request, Response } from 'express';
import { prisma } from '../config/db.js';

const validateEmail = (email: string) => /^\S+@\S+\.\S+$/.test(email);
const validateNif = (nif: string) => /^[0-9XYZ][0-9]{7}[A-Z]$/i.test(nif);

//create customer
//I noticed my program wasn't properly taking into account caps on email addresses so this is the fix I think fits best
export const createCustomer = async (req: Request, res: Response) => {
  let { nif, name, address, phonenumber, email } = req.body;

  if (email) email = email.toLowerCase().trim();
  if (nif) nif = nif.toUpperCase().trim();

  // 1. Check if it's a "Fake" Spanish ID
  if (nif && !idCheck(nif)) {
    return res.status(400).json({ 
      message: "The DNI/NIE format is Spanish, but the control letter is incorrect." 
    });
  }

  if (email && !/^\S+@\S+\.\S+$/.test(email)) {
    return res.status(400).json({ message: "Invalid email format" });
  }

  try {
    const newCustomer = await prisma.customer.create({
      data: { nif, name, address, phonenumber, email },
    });
    return res.status(201).json(newCustomer);
  } catch (error: any) {
    if (error.code === 'P2002') {
      return res.status(400).json({ message: "Email or NIF is incorrect" });
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

    if (!customer) return res.status(404).json({ message: "Customer not found" });
    
    return res.status(200).json(customer);
  } catch (error: any) {
    res.status(500).json({ message: "Error obtaining customer", error: error.message });
  }
};

export const getCustomers = async (req: Request, res: Response) => {
  try {
    const customers = await prisma.customer.findMany({
      include: { vehicles: true } 
    });

    return res.status(200).json(customers);
  } catch (error: any) {
    res.status(500).json({ 
      message: "Error obtaining customers", 
      error: error.message 
    });
  }
};

export const updateCustomer = async (req: Request, res: Response) => {
  const { id } = req.params;
  let { nif, name, address, phonenumber, email } = req.body;

  if (email) email = email.toLowerCase().trim();
  if (nif) nif = nif.toUpperCase().trim();

  if (email && !validateEmail(email)) return res.status(400).json({ message: "Invalid email format" });
  if (nif && !validateNif(nif)) return res.status(400).json({ message: "Invalid NIF format" });

  try {
    const updatedCustomer = await prisma.customer.update({
      where: { id: parseInt(id) },
      data: { nif, name, address, phonenumber, email },
    });

    return res.status(200).json(updatedCustomer);
  } catch (error: any) {
    if (error.code === 'P2002') {
      return res.status(400).json({ message: "NIF or email already in use" });
    }

    if (error.code === 'P2025') return res.status(404).json({ message: "Customer not found" });

    res.status(500).json({ message: "Error on update", error: error.message });
  }
};


//id check
const idCheck = (id: string): boolean => {
  const normalized = id.toUpperCase().trim().replace(/[- ]/g, '');
  
  const nifRegex = /^[0-9]{7,8}[A-Z]$/;
  const nieRegex = /^[XYZ][0-9]{7}[A-Z]$/;

  const isNif = nifRegex.test(normalized);
  const isNie = nieRegex.test(normalized);

  if (!isNif && !isNie) {
    return true; 
  }

  const mapping = "TRWAGMYFPDXBNJZSQVHLCKE";
  
  const letter = normalized.slice(-1);
  let numberPart = normalized.slice(0, -1);

  if (isNie) {
    numberPart = numberPart
      .replace('X', '0')
      .replace('Y', '1')
      .replace('Z', '2');
  }

  const digits = parseInt(numberPart, 10);
  const expectedLetter = mapping[digits % 23];

  return letter === expectedLetter;
};