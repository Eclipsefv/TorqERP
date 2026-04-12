import { Router } from 'express';
import { createWorkOrder,getWorkOrders,convertToInvoice,getWorkOrderById,addLineToWorkOrder,updateWorkOrder } from '../controllers/workOrderController.js';
import { authenticateToken } from '../middlewares/authMiddleware.js';

const router = Router();

router.post('/insert', authenticateToken, createWorkOrder);
router.get('/getWorkOrders', getWorkOrders);
router.get('/getWorkOrder/:id', getWorkOrderById);
router.post('/addLine', addLineToWorkOrder)
router.put('/update/:id', updateWorkOrder);
router.put('/convertToInvoice/:id',convertToInvoice)

export default router;