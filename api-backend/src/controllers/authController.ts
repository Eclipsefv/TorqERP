import { Request, Response } from 'express';
import bcrypt from 'bcrypt';
import jwt from 'jsonwebtoken';
import prisma from '../config/db';

//REGISTER
export const register = async (req: Request, res: Response): Promise<any> => {
  try {
    const { email, password, username, role } = req.body; 
    
    const hashedPassword = await bcrypt.hash(password, 10);
    
    const user = await prisma.user.create({
      data: { 
        email, 
        password: hashedPassword, 
        username,
        role
      }
    });
    
    const { password: _, ...userNoPass } = user;
    res.status(201).json(userNoPass);
  } catch (e) {
    console.error(e);
    res.status(500).json({ error: "Server error" });
  }
};

//LOGIN
export const login = async (req: Request, res: Response): Promise<any> => {
  try {
    const { email, password } = req.body;
    
    const user = await prisma.user.findUnique({ where: { email } });

    if (!user || !(await bcrypt.compare(password, user.password))) {
      return res.status(401).json({ error: "invalid credentials" });
    }

    const token = jwt.sign(
      { id: user.id, role: user.role }, 
      process.env.JWT_SECRET || 'torq_secret', 
      { expiresIn: '8h' }
    );

    res.json({ 
      token, 
      user: { 
        id: user.id, 
        email: user.email,
        role: user.role
      } 
    });

  } catch (e) {
    res.status(500).json({ error: "Error on login" });
  }
};