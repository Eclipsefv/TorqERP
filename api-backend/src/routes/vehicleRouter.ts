import { Router } from 'express';
import { insertVehicle,getVehicles,updateVehicle } from '../controllers/vehicleController.js';
import { authenticateToken } from '../middlewares/authMiddleware.js';

const router = Router();

router.post('/insert', authenticateToken, insertVehicle);
router.get('/getVehicles', authenticateToken, getVehicles);
router.put('/updateVehicle/:id', authenticateToken, updateVehicle)

export default router;