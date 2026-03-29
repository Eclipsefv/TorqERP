import { Router } from 'express';
import {
  requestOtp,
  verifyOtp,
  verifySessionToken,
  getMyAppointments,
  createMyAppointment,
  deleteMyAppointment,
  getUnavailableTimes,
} from '../controllers/publicController.js';

const router = Router();

router.post('/request-otp', requestOtp);
router.post('/verify-otp', verifyOtp);

router.get('/appointments/unavailable-times', verifySessionToken, getUnavailableTimes);
router.get('/appointments', verifySessionToken, getMyAppointments);
router.post('/appointments', verifySessionToken, createMyAppointment);
router.delete('/appointments/:id', verifySessionToken, deleteMyAppointment);

export default router;
