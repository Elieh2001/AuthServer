import './Login.css';
import LoginForm from './LoginForm';


export default function LoginPage() {
return (
<div className="login-container">
{/* Left Image */}
<div className="login-left" />


{/* Right Form */}
<div className="login-right">
<LoginForm />
</div>
</div>
);
}