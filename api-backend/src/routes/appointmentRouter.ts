import { Router } from 'express';
import { getAppointments, createAppointment } from '../controllers/appointmentController';

const router = Router();

console.log("Controlador getAppointments:", getAppointments);
router.get('/getAllAppointments', getAppointments);
router.post('/createAppointment', createAppointment);

export default router;