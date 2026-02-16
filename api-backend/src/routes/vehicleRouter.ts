import { Router } from 'express';
import { insertVehicle } from '../controllers/vehicleController.js';
import { authenticateToken } from '../middlewares/authMiddleware.js';

const router = Router();

router.post('/insert', authenticateToken, insertVehicle);

export default router;