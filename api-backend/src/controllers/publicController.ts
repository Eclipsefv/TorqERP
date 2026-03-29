import { Request, Response } from 'express';
import jwt from 'jsonwebtoken';
import { prisma } from '../config/db.js';
import { sendOtpEmail } from '../services/emailService.js';

const APP_JWT_SECRET = process.env.APP_JWT_SECRET || process.env.JWT_SECRET || 'app-session-secret-change-me';

const generateOtp = (): string => {
  return Math.floor(100000 + Math.random() * 900000).toString();
};

const getMadridTime = (date: Date) => {
  const formatter = new Intl.DateTimeFormat('en-GB', {
    timeZone: 'Europe/Madrid',
    hour: 'numeric',
    minute: 'numeric',
  });
  const timeString = formatter.format(date);
  const parts = timeString.split(':');
  
  const hNum = parseInt((parts[0] || '0').replace(/\D/g, ''), 10);
  const mNum = parseInt((parts[1] || '0').replace(/\D/g, ''), 10);
  
  const hStr = hNum.toString().padStart(2, '0');
  const mStr = mNum.toString().padStart(2, '0');
  
  return {
    hour: hNum,
    minute: mNum,
    timeStr: `${hStr}:${mStr}`
  };
};

export const requestOtp = async (req: Request, res: Response) => {
  try {
    let { nif, email } = req.body;

    if (!nif || !email) {
      return res.status(400).json({ message: 'ID and Email are required' });
    }

    nif = nif.toUpperCase().trim();
    const cleanEmail = email.toLowerCase().trim();

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
    
    if (customer.email.toLowerCase() !== cleanEmail) {
      return res.status(400).json({ message: 'The provided email does not match our records for this ID' });
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
    
    // Enforce exact hourly slots (e.g. 09:00:00.000)
    scheduledDate.setMinutes(0, 0, 0);

    if (scheduledDate <= new Date()) {
      return res.status(400).json({ message: 'Appointment date must be in the future' });
    }

    const madridTime = getMadridTime(scheduledDate);
    if (madridTime.hour < 9 || madridTime.hour > 15 || (madridTime.hour === 15 && madridTime.minute > 0)) {
      return res.status(400).json({ message: 'Appointments can only be booked between 9:00 and 15:00' });
    }

    // Check if this specific hour is already booked
    const conflictingAppointment = await prisma.appointment.findFirst({
      where: {
        scheduledAt: scheduledDate,
      },
    });

    if (conflictingAppointment) {
      return res.status(400).json({ message: 'This time slot is already booked. Please choose another one.' });
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

export const getUnavailableTimes = async (req: Request, res: Response) => {
  try {
    const { date } = req.query;

    if (!date || typeof date !== 'string') {
      return res.status(400).json({ message: 'Date parameter is required in YYYY-MM-DD format (e.g. ?date=2026-03-29)' });
    }

    const queryDate = new Date(date);
    if (isNaN(queryDate.getTime())) {
      return res.status(400).json({ message: 'Invalid date format' });
    }

    const dayStart = new Date(queryDate);
    dayStart.setHours(0, 0, 0, 0);
    const dayEnd = new Date(queryDate);
    dayEnd.setHours(23, 59, 59, 999);

    const appointments = await prisma.appointment.findMany({
      where: {
        scheduledAt: {
          gte: dayStart,
          lte: dayEnd,
        },
      },
      select: {
        scheduledAt: true,
      },
    });

    // If day is fully booked via the limit constraint
    if (appointments.length >= 5) {
      // Return all possible hours to block the whole day
      return res.status(200).json(["09:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00"]);
    }

    const bookedTimes = appointments.map((appt: any) => {
      return getMadridTime(appt.scheduledAt).timeStr;
    });

    return res.status(200).json(bookedTimes);

  } catch (error: any) {
    console.error('Error in getUnavailableTimes:', error);
    return res.status(500).json({ 
      message: 'Error getting unavailable times', 
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
