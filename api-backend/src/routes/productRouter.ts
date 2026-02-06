import { Router } from 'express';
import { insertProduct,getProducts } from '../controllers/productController.js';
import { authenticateToken } from '../middlewares/authMiddleware.js';

const router = Router();

router.post('/insert', authenticateToken, insertProduct);
router.get('/getProducts', authenticateToken, getProducts);

export default router;