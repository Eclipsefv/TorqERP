import { Router } from 'express';
import { getUsers,deleteUserById } from '../controllers/userController.js';
import { authenticateToken } from '../middlewares/authMiddleware.js';

const router = Router();

router.get('/getUsers', authenticateToken, getUsers);
router.delete('/deleteUserById/:id', authenticateToken, deleteUserById);

export default router;