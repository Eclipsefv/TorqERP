import { Request, Response } from 'express';
import { prisma } from '../config/db.js';


export const getUsers = async (req: Request, res: Response) => {
  try {
    const users = await prisma.user.findMany({
      where: {
        active: true,
      },
      orderBy: { id: 'asc' },
    });

    return res.status(200).json(users);
  } catch (error: any) {  
    res.status(500).json({ message: "Error obtaining users", error: error.message });
  }
};


//"DELETE" ENDPOINT RECEIVES ID
export const deleteUserById = async (req: Request, res: Response) => {
  const { id } = req.params;
  const userId = parseInt(id);

  try {
    const deactivatedUser = await prisma.user.update({
      where: { id: userId },
      data: { active: false },
    });

    return res.status(200).json({
      message: "User deactivated",
      user: deactivatedUser,
    });
  } catch (error: any) {
    if (error.code === 'P2025') {
      return res.status(404).json({ message: "User not found" });
    }
    return res.status(500).json({ message: "Error on deactivation", error: error.message });
  }
};