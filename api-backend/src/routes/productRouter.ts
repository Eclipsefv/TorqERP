import { Router } from 'express';
import { insertProduct,getProducts,updateProduct } from '../controllers/productController.js';
import { authenticateToken } from '../middlewares/authMiddleware.js';

const router = Router();

router.post('/insert', authenticateToken, insertProduct);
router.get('/getProducts', authenticateToken, getProducts);
router.put('/updateProduct/:id', authenticateToken, updateProduct)

export default router;