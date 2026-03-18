import { Router } from 'express';
import {
  requestOtp,
  verifyOtp,
  verifySessionToken,
  getMyAppointments,
  createMyAppointment,
} from '../controllers/publicController.js';

const router = Router();

// Rutas públicas (sin JWT de administrador)
router.post('/request-otp', requestOtp);
router.post('/verify-otp', verifyOtp);

// Rutas protegidas por token de sesión OTP (no requieren JWT de admin)
router.get('/appointments', verifySessionToken, getMyAppointments);
router.post('/appointments', verifySessionToken, createMyAppointment);

export default router;

// ====================================================
// INSTRUCCIONES DE INTEGRACIÓN:
// 
// En tu archivo principal (server.ts / app.ts / index.ts), añade:
//
//   import publicRoutes from './routes/publicRoutes.js';
//   app.use('/api/public', publicRoutes);
//
// Asegúrate de que esta línea va ANTES de cualquier
// middleware de autenticación global.
//
// Variables de entorno necesarias (.env):
//   SMTP_HOST=smtp.gmail.com
//   SMTP_PORT=587
//   SMTP_USER=tu-email@gmail.com
//   SMTP_PASS=tu-app-password
//   SMTP_FROM="Taller App" <tu-email@gmail.com>
//   APP_JWT_SECRET=una-clave-secreta-larga
//
// Dependencia necesaria (si no la tienes):
//   npm install nodemailer
//   npm install -D @types/nodemailer
// ====================================================
