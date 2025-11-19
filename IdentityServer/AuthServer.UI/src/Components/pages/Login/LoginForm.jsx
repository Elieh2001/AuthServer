import React, { useState } from 'react';
import LoginField from './LoginField';
import MailOutlineIcon from '@mui/icons-material/MailOutline';
import LockOutlinedIcon from '@mui/icons-material/LockOutlined';


export default function LoginForm() {
const [tenant, setTenant] = useState('');
const [email, setEmail] = useState('');
const [password, setPassword] = useState('');


return (
<div className="form-wrapper">
<h1 className="form-title">SIGN IN</h1>


<label className="field-label">Tenant</label>
<LoginField
placeholder="Tenant"
icon={<MailOutlineIcon />}
value={tenant}
onChange={(e) => setTenant(e.target.value)}
/>


<label className="field-label">Email</label>
<LoginField
placeholder="Yourname@gmail.com"
icon={<MailOutlineIcon />}
value={email}
onChange={(e) => setEmail(e.target.value)}
/>


<label className="field-label">Password</label>
<LoginField
placeholder="Password"
type="password"
icon={<LockOutlinedIcon />}
value={password}
onChange={(e) => setPassword(e.target.value)}
/>


<button className="login-btn">Login</button>
</div>
);
}