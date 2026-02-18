import { Router } from 'express';
import { insertVehicle,getVehicles } from '../controllers/vehicleController.js';
import { authenticateToken } from '../middlewares/authMiddleware.js';

const router = Router();

router.post('/insert', authenticateToken, insertVehicle);
router.get('/getVehicles', authenticateToken, getVehicles);

export default router;