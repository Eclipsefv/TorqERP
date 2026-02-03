import { Request, Response } from 'express';
import { prisma } from '../config/db.js';


export const getUsers = async (req: Request, res: Response) => {
  try {
    const users = await prisma.user.findMany({
      orderBy: {
        id: 'asc',
      },
    });

    return res.status(200).json(users);
  } catch (error: any) {
    res.status(500).json({ 
      message: "Error obtaining users", 
      error: error.message 
    });
  }
};