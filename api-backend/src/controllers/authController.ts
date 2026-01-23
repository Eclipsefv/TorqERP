import { Request, Response } from 'express';
import bcrypt from 'bcrypt';
import jwt from 'jsonwebtoken';
import prisma from '../config/db';

//REGISTER
export const register = async (req: Request, res: Response): Promise<any> => {
  try {
    const { email, password, name } = req.body;
    const hashedPassword = await bcrypt.hash(password, 10);
    const user = await prisma.user.create({
      data: { email, password: hashedPassword, name }
    });
    const { password: _, ...userNoPass } = user;
    res.status(201).json(userNoPass);
  } catch (e) {
    res.status(500).json({ error: "Email ya registrado o error de DB" });
  }
};

//LOGIN
export const login = async (req: Request, res: Response): Promise<any> => {
  try {
    const { email, password } = req.body;
    const user = await prisma.user.findUnique({ where: { email } });
    if (!user || !(await bcrypt.compare(password, user.password))) {
      return res.status(401).json({ error: "Credenciales inválidas" });
    }
    const token = jwt.sign({ id: user.id }, process.env.JWT_SECRET || 'torq_secret', { expiresIn: '8h' });
    res.json({ token, user: { id: user.id, email: user.email } });
  } catch (e) {
    res.status(500).json({ error: "Error en el login" });
  }
};