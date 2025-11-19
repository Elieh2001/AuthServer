import React from 'react';
import { TextField, InputAdornment } from '@mui/material';


export default function LoginField({ placeholder, icon, value, onChange, type }) {
return (
<TextField
fullWidth
type={type || 'text'}
placeholder={placeholder}
value={value}
onChange={onChange}
variant="outlined"
InputProps={{
sx: {
backgroundColor: '#150033',
borderRadius: '10px',
color: '#fff',
},
startAdornment: (
<InputAdornment position="start">
<span className="input-icon">{icon}</span>
</InputAdornment>
),
}}
className="login-input"
/>
);
}