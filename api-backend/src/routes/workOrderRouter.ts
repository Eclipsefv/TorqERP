import { Router } from 'express';
import { createWorkOrder } from '../controllers/workOrderController.js';
import { authenticateToken } from '../middlewares/authMiddleware.js';

const router = Router();

router.post('/insert', authenticateToken, createWorkOrder);

export default router;