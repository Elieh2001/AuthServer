// Card.js
import React from 'react';
import { useTheme } from '@mui/material/styles';


const Card = ({ data }) => {
    const theme = useTheme();

    const cardStyle = {
        backgroundColor:  theme.palette.primary.main,
        color: theme.palette.text.primary,
        boxShadow: '0 4px 8px rgba(0, 0, 0, 0.1)',
    };

    return (
        <div className="card" style={cardStyle}>
            <h2>{data.value}</h2>
            <p>{data.text}</p>
        </div>
    );
};

export default Card;
