import nodemailer from 'nodemailer';

// ====================================================
// Configura aquí tus credenciales de email
// Puedes usar Gmail (con app password), Outlook, etc.
// ====================================================
const transporter = nodemailer.createTransport({
  host: process.env.SMTP_HOST || 'smtp.gmail.com',
  port: Number(process.env.SMTP_PORT) || 587,
  secure: false,
  auth: {
    user: process.env.SMTP_USER || 'textodeejemplozzz@gmail.com',
    pass: process.env.SMTP_PASS || 'tu-app-password',
  },
});

export const sendOtpEmail = async (to: string, otpCode: string): Promise<void> => {
  const mailOptions = {
    from: process.env.SMTP_FROM || '"Taller App" <tu-email@gmail.com>',
    to,
    subject: 'Tu código de verificación - Taller',
    html: `
      <div style="font-family: 'Segoe UI', Arial, sans-serif; max-width: 480px; margin: 0 auto; background: #f8f9fa; border-radius: 12px; overflow: hidden;">
        <div style="background: linear-gradient(135deg, #1A237E, #283593); padding: 32px 24px; text-align: center;">
          <h1 style="color: #ffffff; margin: 0; font-size: 24px;">🔧 Taller App</h1>
          <p style="color: #b3b9ff; margin: 8px 0 0;">Verificación de identidad</p>
        </div>
        <div style="padding: 32px 24px; text-align: center;">
          <p style="color: #333; font-size: 16px; margin-bottom: 24px;">
            Tu código de verificación es:
          </p>
          <div style="background: #1A237E; color: #ffffff; font-size: 36px; font-weight: bold; letter-spacing: 12px; padding: 20px 32px; border-radius: 12px; display: inline-block;">
            ${otpCode}
          </div>
          <p style="color: #666; font-size: 14px; margin-top: 24px;">
            Este código expira en <strong>5 minutos</strong>.<br/>
            Si no has solicitado este código, ignora este email.
          </p>
        </div>
        <div style="background: #e8eaf6; padding: 16px 24px; text-align: center;">
          <p style="color: #999; font-size: 12px; margin: 0;">
            © ${new Date().getFullYear()} Taller App — Gestión de citas
          </p>
        </div>
      </div>
    `,
  };

  await transporter.sendMail(mailOptions);
};
