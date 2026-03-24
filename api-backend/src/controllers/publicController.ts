import { Request, Response } from 'express';
import jwt from 'jsonwebtoken';
import { prisma } from '../config/db.js';
import { sendOtpEmail } from '../services/emailService.js';

const APP_JWT_SECRET = process.env.APP_JWT_SECRET || process.env.JWT_SECRET || 'app-session-secret-change-me';

const generateOtp = (): string => {
  return Math.floor(100000 + Math.random() * 900000).toString();
};

export const requestOtp = async (req: Request, res: Response) => {
  try {
    let { nif } = req.body;

    if (!nif) {
      return res.status(400).json({ message: 'ID is required' });
    }

    nif = nif.toUpperCase().trim();

    const customer = await prisma.customer.findUnique({
      where: { nif },
    });

    if (!customer) {
      return res.status(404).json({ message: 'No client found with that ID' });
    }

    if (!customer.email) {
      return res.status(400).json({ 
        message: 'You don\'t have a registered email. Contact the workshop to set it up.' 
      });
    }

    const otpCode = generateOtp();
    const otpExpires = new Date(Date.now() + 5 * 60 * 1000);

    await prisma.customer.update({
      where: { id: customer.id },
      data: { otpCode, otpExpires },
    });

    await sendOtpEmail(customer.email, otpCode);

    const emailParts = customer.email.split('@');
    const maskedEmail = emailParts[0].substring(0, 2) + '***@' + emailParts[1];

    return res.status(200).json({
      message: `Verification code sent to ${maskedEmail}`,
      maskedEmail,
    });

  } catch (error: any) {
    console.error('Error in requestOtp:', error);
    return res.status(500).json({ 
      message: 'Error sending the verification code', 
      error: error.message 
    });
  }
};

export const verifyOtp = async (req: Request, res: Response) => {
  try {
    let { nif, otpCode } = req.body;

    if (!nif || !otpCode) {
      return res.status(400).json({ message: 'ID and OTP Code are required' });
    }

    nif = nif.toUpperCase().trim();

    const customer = await prisma.customer.findUnique({
      where: { nif },
      include: { vehicles: true },
    });

    if (!customer) {
      return res.status(404).json({ message: 'Client not found' });
    }

    if (!customer.otpCode || customer.otpCode !== otpCode) {
      return res.status(401).json({ message: 'Incorrect verification code' });
    }

    if (!customer.otpExpires || new Date() > customer.otpExpires) {
      return res.status(401).json({ message: 'The code has expired. Request a new one.' });
    }

    await prisma.customer.update({
      where: { id: customer.id },
      data: { otpCode: null, otpExpires: null },
    });

    const sessionToken = jwt.sign(
      { customerId: customer.id, type: 'app-session' },
      APP_JWT_SECRET,
      { expiresIn: '1h' }
    );

    return res.status(200).json({
      sessionToken,
      customer: {
        id: customer.id,
        name: customer.name,
        nif: customer.nif,
        email: customer.email,
      },
      vehicles: customer.vehicles.map((v) => ({
        id: v.id,
        plate: v.plate,
        brand: v.brand,
        model: v.model,
        year: v.year,
      })),
    });

  } catch (error: any) {
    console.error('Error in verifyOtp:', error);
    return res.status(500).json({ 
      message: 'Error verifying the code', 
      error: error.message 
    });
  }
};

export const verifySessionToken = (req: Request, res: Response, next: Function) => {
  const authHeader = req.headers.authorization;

  if (!authHeader || !authHeader.startsWith('Bearer ')) {
    return res.status(401).json({ message: 'Session token required' });
  }

  const token = authHeader.split(' ')[1];

  try {
    const decoded = jwt.verify(token, APP_JWT_SECRET) as any;
    
    if (decoded.type !== 'app-session') {
      return res.status(401).json({ message: 'Invalid session token' });
    }

    (req as any).sessionData = {
      customerId: decoded.customerId,
    };

    next();
  } catch (error) {
    return res.status(401).json({ message: 'Session expired. Please log in again.' });
  }
};

export const getMyAppointments = async (req: Request, res: Response) => {
  try {
    const { customerId } = (req as any).sessionData;

    const appointments = await prisma.appointment.findMany({
      where: {
        customerId: customerId,
      },
      include: {
        vehicle: true,
      },
      orderBy: {
        scheduledAt: 'desc',
      },
    });

    return res.status(200).json(appointments);

  } catch (error: any) {
    console.error('Error in getMyAppointments:', error);
    return res.status(500).json({ 
      message: 'Error getting appointments', 
      error: error.message 
    });
  }
};

export const createMyAppointment = async (req: Request, res: Response) => {
  try {
    const { customerId } = (req as any).sessionData;
    const { vehicleId, scheduledAt, description } = req.body;

    if (!vehicleId) {
      return res.status(400).json({ message: 'Vehicle ID is required' });
    }

    if (!scheduledAt) {
      return res.status(400).json({ message: 'Appointment date (scheduledAt) is required' });
    }

    const vehicle = await prisma.vehicle.findFirst({
      where: {
        id: vehicleId,
        customerId: customerId,
      },
    });

    if (!vehicle) {
      return res.status(404).json({ message: 'Vehicle not found or does not belong to this customer' });
    }

    const scheduledDate = new Date(scheduledAt);
    
    if (scheduledDate <= new Date()) {
      return res.status(400).json({ message: 'Appointment date must be in the future' });
    }

    const scheduledHour = scheduledDate.getHours();
    if (scheduledHour < 9 || scheduledHour >= 16) {
      return res.status(400).json({ message: 'Appointments can only be booked between 9:00 and 16:00' });
    }

    const dayStart = new Date(scheduledDate);
    dayStart.setHours(0, 0, 0, 0);
    const dayEnd = new Date(scheduledDate);
    dayEnd.setHours(23, 59, 59, 999);

    const dailyCount = await prisma.appointment.count({
      where: {
        scheduledAt: {
          gte: dayStart,
          lte: dayEnd,
        },
      },
    });

    if (dailyCount >= 5) {
      return res.status(400).json({ message: 'The daily appointment limit (5) has been reached for this date' });
    }

    const newAppointment = await prisma.appointment.create({
      data: {
        scheduledAt: scheduledDate,
        description: description || null,
        status: 'SCHEDULED',
        customer: { connect: { id: customerId } },
        vehicle: { connect: { id: vehicleId } },
      },
      include: {
        vehicle: true,
      },
    });

    return res.status(201).json(newAppointment);

  } catch (error: any) {
    console.error('Error in createMyAppointment:', error);
    return res.status(500).json({ 
      message: 'Error creating appointment', 
      error: error.message 
    });
  }
};

export const deleteMyAppointment = async (req: Request, res: Response) => {
  try {
    const { customerId } = (req as any).sessionData;
    const appointmentId = parseInt(req.params.id);

    if (isNaN(appointmentId)) {
      return res.status(400).json({ message: 'Invalid appointment ID' });
    }

    const appointment = await prisma.appointment.findFirst({
      where: {
        id: appointmentId,
        customerId: customerId,
      },
    });

    if (!appointment) {
      return res.status(404).json({ message: 'Appointment not found' });
    }

    if (appointment.status === 'COMPLETED') {
      return res.status(400).json({ message: 'Cannot delete a completed appointment' });
    }

    await prisma.appointment.delete({
      where: { id: appointmentId },
    });

    return res.status(204).send();

  } catch (error: any) {
    console.error('Error in deleteMyAppointment:', error);
    return res.status(500).json({ 
      message: 'Error deleting appointment', 
      error: error.message 
    });
  }
};
