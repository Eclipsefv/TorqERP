import { Router } from 'express';
import { getAppointments, createAppointment } from '../controllers/appointmentController.js';

const router = Router();

router.get('/getAllAppointments', getAppointments);
router.post('/createAppointment', createAppointment);

export default router;