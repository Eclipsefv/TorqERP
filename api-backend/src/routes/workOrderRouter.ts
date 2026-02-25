import { Router } from 'express';
import { createWorkOrder,getWorkOrders,getWorkOrderById } from '../controllers/workOrderController.js';
import { authenticateToken } from '../middlewares/authMiddleware.js';

const router = Router();

router.post('/insert', authenticateToken, createWorkOrder);
router.get('/getWorkOrders', getWorkOrders);
router.get('/getWorkOrder/:id', getWorkOrderById);

export default router;