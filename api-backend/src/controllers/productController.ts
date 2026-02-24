import { Request, Response } from 'express';
import { prisma } from '../config/db.js';

export const insertProduct = async (req: Request, res: Response): Promise<any> => {
  try {
    const { 
      sku, name, description, type, 
      buyPrice, sellPrice, taxRate, 
      stock, minStock, location 
    } = req.body;

    if (!sku || !name) {
      return res.status(400).json({ message: "SKU and name are required" });
    }

    const existingProduct = await prisma.product.findFirst({
  where: { sku: sku }
});

    if (existingProduct) {
      return res.status(409).json({ message: "SKU already on db" });
    }

    const newProduct = await prisma.product.create({
      data: {
        sku,
        name,
        description,
        type: type || 'ITEM',
        buyPrice: buyPrice || 0.0,
        sellPrice: sellPrice || 0.0,
        taxRate: taxRate || 21.0,
        stock: stock || 0,
        minStock: minStock || 0,
        location
      },
    });

    res.status(201).json(newProduct);

  } catch (error: any) {
    console.error("Error inserting product:", error);

    res.status(500).json({ message: "Internal server error", error: error.message });
  }
};

export const getProducts = async (req: Request, res: Response) => {
  try {
    const products = await prisma.product.findMany({
      orderBy: {
        updatedAt: 'desc',
      },
    });

    return res.status(200).json(products);
  } catch (error: any) {
    res.status(500).json({ 
      message: "Error obtaining products", 
      error: error.message 
    });
  }
};

export const updateProduct = async (req: Request, res: Response) => {
  try {
    const { id } = req.params; 
    
    const { 
      sku, name, description, type, 
      buyPrice, sellPrice, taxRate, 
      stock, minStock, location 
    } = req.body;
    if (!id) {
      return res.status(400).json({ message: "Product ID is required in params" });
    }

    const updatedProduct = await prisma.product.update({
      where: {
        id: Number(id),
      },
      data: {
        sku,
        name,
        description,
        type,
        buyPrice,
        sellPrice,
        taxRate,
        stock,
        minStock,
        location,
      },
    });

    res.status(200).json(updatedProduct);

  } catch (error: any) {
    if (error.code === 'P2025') {
      return res.status(404).json({ message: "Product not found" });
    }
    
    if (error.code === 'P2002') {
      return res.status(400).json({ message: "The new sku is already in use" });
    }

    res.status(500).json({ error: error.message });
  }
};