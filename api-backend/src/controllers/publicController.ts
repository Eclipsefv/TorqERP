import { Request, Response } from 'express';
import jwt from 'jsonwebtoken';
import { prisma } from '../config/db.js';
import { sendOtpEmail } from '../services/emailService.js';

// ====================================================
// Clave secreta para los JWT de sesión de la app móvil.
// Puedes reusar tu JWT_SECRET existente o usar una distinta.
// ====================================================
const APP_JWT_SECRET = process.env.APP_JWT_SECRET || process.env.JWT_SECRET || 'app-session-secret-change-me';

// Genera un código OTP de 6 dígitos
const generateOtp = (): string => {
  return Math.floor(100000 + Math.random() * 900000).toString();
};

/**
 * POST /api/public/request-otp
 * Body: { nif: string, plate: string }
 * 
 * Busca un customer con ese NIF que tenga un vehicle con esa matrícula.
 * Genera OTP, lo guarda en BD y lo envía por email.
 */
export const requestOtp = async (req: Request, res: Response) => {
  try {
    let { nif, plate } = req.body;

    if (!nif || !plate) {
      return res.status(400).json({ message: 'Se requieren NIF y matrícula' });
    }

    nif = nif.toUpperCase().trim();
    plate = plate.toUpperCase().trim().replace(/[\s-]/g, '');

    // Buscar customer por NIF
    const customer = await prisma.customer.findUnique({
      where: { nif },
      include: { vehicles: true },
    });

    if (!customer) {
      return res.status(404).json({ message: 'No se encontró ningún cliente con ese DNI/NIF' });
    }

    // Verificar que el customer tiene un vehículo con esa matrícula
    const vehicle = customer.vehicles.find(
      (v) => v.plate.toUpperCase().replace(/[\s-]/g, '') === plate
    );

    if (!vehicle) {
      return res.status(404).json({ message: 'No se encontró un vehículo con esa matrícula asociado a tu DNI' });
    }

    // Verificar que el customer tiene email para recibir el OTP
    if (!customer.email) {
      return res.status(400).json({ 
        message: 'No tienes un email registrado. Contacta con el taller para configurarlo.' 
      });
    }

    // Generar OTP y guardar en BD
    const otpCode = generateOtp();
    const otpExpires = new Date(Date.now() + 5 * 60 * 1000); // 5 minutos

    await prisma.customer.update({
      where: { id: customer.id },
      data: { otpCode, otpExpires },
    });

    // Enviar OTP por email
    await sendOtpEmail(customer.email, otpCode);

    // Enmascarar email para la respuesta
    const emailParts = customer.email.split('@');
    const maskedEmail = emailParts[0].substring(0, 2) + '***@' + emailParts[1];

    return res.status(200).json({
      message: `Código de verificación enviado a ${maskedEmail}`,
      maskedEmail,
    });

  } catch (error: any) {
    console.error('Error en requestOtp:', error);
    return res.status(500).json({ 
      message: 'Error al enviar el código de verificación', 
      error: error.message 
    });
  }
};

/**
 * POST /api/public/verify-otp
 * Body: { nif: string, plate: string, otpCode: string }
 * 
 * Verifica el código OTP y devuelve un JWT de sesión + datos del customer/vehicle.
 */
export const verifyOtp = async (req: Request, res: Response) => {
  try {
    let { nif, plate, otpCode } = req.body;

    if (!nif || !otpCode || !plate) {
      return res.status(400).json({ message: 'Se requieren NIF, matrícula y código OTP' });
    }

    nif = nif.toUpperCase().trim();
    plate = plate.toUpperCase().trim().replace(/[\s-]/g, '');

    const customer = await prisma.customer.findUnique({
      where: { nif },
      include: { vehicles: true },
    });

    if (!customer) {
      return res.status(404).json({ message: 'Cliente no encontrado' });
    }

    // Verificar OTP
    if (!customer.otpCode || customer.otpCode !== otpCode) {
      return res.status(401).json({ message: 'Código de verificación incorrecto' });
    }

    // Verificar que no ha expirado
    if (!customer.otpExpires || new Date() > customer.otpExpires) {
      return res.status(401).json({ message: 'El código ha expirado. Solicita uno nuevo.' });
    }

    // Buscar vehículo
    const vehicle = customer.vehicles.find(
      (v) => v.plate.toUpperCase().replace(/[\s-]/g, '') === plate
    );

    if (!vehicle) {
      return res.status(404).json({ message: 'Vehículo no encontrado para este cliente' });
    }

    // Limpiar OTP de la BD
    await prisma.customer.update({
      where: { id: customer.id },
      data: { otpCode: null, otpExpires: null },
    });

    // Generar JWT de sesión (1 hora)
    const sessionToken = jwt.sign(
      { customerId: customer.id, vehicleId: vehicle.id, type: 'app-session' },
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
      vehicle: {
        id: vehicle.id,
        plate: vehicle.plate,
        brand: vehicle.brand,
        model: vehicle.model,
        year: vehicle.year,
      },
    });

  } catch (error: any) {
    console.error('Error en verifyOtp:', error);
    return res.status(500).json({ 
      message: 'Error al verificar el código', 
      error: error.message 
    });
  }
};

/**
 * Middleware para verificar el JWT de sesión de la app móvil
 */
export const verifySessionToken = (req: Request, res: Response, next: Function) => {
  const authHeader = req.headers.authorization;

  if (!authHeader || !authHeader.startsWith('Bearer ')) {
    return res.status(401).json({ message: 'Token de sesión requerido' });
  }

  const token = authHeader.split(' ')[1];

  try {
    const decoded = jwt.verify(token, APP_JWT_SECRET) as any;
    
    if (decoded.type !== 'app-session') {
      return res.status(401).json({ message: 'Token de sesión inválido' });
    }

    (req as any).sessionData = {
      customerId: decoded.customerId,
      vehicleId: decoded.vehicleId,
    };

    next();
  } catch (error) {
    return res.status(401).json({ message: 'Sesión expirada. Vuelve a identificarte.' });
  }
};

/**
 * GET /api/public/appointments
 * Headers: Authorization: Bearer <sessionToken>
 * 
 * Devuelve las citas del customer autenticado por OTP.
 */
export const getMyAppointments = async (req: Request, res: Response) => {
  try {
    const { customerId, vehicleId } = (req as any).sessionData;

    const appointments = await prisma.appointment.findMany({
      where: {
        customerId: customerId,
        vehicleId: vehicleId,
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
    console.error('Error en getMyAppointments:', error);
    return res.status(500).json({ 
      message: 'Error al obtener las citas', 
      error: error.message 
    });
  }
};

/**
 * POST /api/public/appointments
 * Headers: Authorization: Bearer <sessionToken>
 * Body: { scheduledAt: string, description?: string }
 * 
 * Crea una nueva cita para el customer/vehicle de la sesión.
 */
export const createMyAppointment = async (req: Request, res: Response) => {
  try {
    const { customerId, vehicleId } = (req as any).sessionData;
    const { scheduledAt, description } = req.body;

    if (!scheduledAt) {
      return res.status(400).json({ message: 'Se requiere la fecha de la cita (scheduledAt)' });
    }

    const scheduledDate = new Date(scheduledAt);
    
    // Validar que la fecha sea futura
    if (scheduledDate <= new Date()) {
      return res.status(400).json({ message: 'La fecha de la cita debe ser en el futuro' });
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
    console.error('Error en createMyAppointment:', error);
    return res.status(500).json({ 
      message: 'Error al crear la cita', 
      error: error.message 
    });
  }
};
