import { Router } from 'express';
import { getUsers } from '../controllers/userController.js';
import { authenticateToken } from '../middlewares/authMiddleware.js';

const router = Router();

router.get('/getUsers', authenticateToken, getUsers);

export default router;