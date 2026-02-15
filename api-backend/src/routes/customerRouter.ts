import { Router } from 'express';
import { createCustomer,getCustomerById,getCustomers} from '../controllers/customerController.js';
import { authenticateToken } from '../middlewares/authMiddleware.js';

const router = Router();

router.get('/getCustomerById/:id', authenticateToken, getCustomerById);
router.post('/insert', authenticateToken, createCustomer);
router.get('/getCustomers', authenticateToken, getCustomers);

export default router;