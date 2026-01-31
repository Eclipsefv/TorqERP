import { Router } from 'express';
import { insertProduct } from '../controllers/productController.js';
import { authenticateToken } from '../middlewares/authMiddleware.js';

const router = Router();

router.post('/insert', authenticateToken, insertProduct);

export default router;