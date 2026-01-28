import { Router } from 'express';
import { register } from '../controllers/authController.js';
import { login } from '../controllers/authController.js';

const router = Router();

//  POST for register
router.post('/register', register);
// POST for login 
router.post('/login', login);

export default router;