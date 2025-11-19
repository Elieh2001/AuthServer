import React from 'react';
import { styled } from '@mui/material/styles';
import { useTheme } from '@mui/material/styles';
import { bordersRadius } from '../../../Common/Common';

const CardContainer = styled('div')(({ theme }) => ({
    display: 'inline-flex',
    width: "50%",
    gap: '20px', 
    //Mobile view
    [theme.breakpoints.down('md')]: {
        width: '100%', 
    },
}));

const StyledCard = styled('div')(({ theme }) => ({
    backgroundColor: theme.palette.primary.main,
    color: theme.palette.text.primary,
    width: '100%',
    padding: '20px',
    position: 'relative',
    //desktop view 
    [theme.breakpoints.up('md')]: {
        margin: '20px', 
        borderRadius: `${bordersRadius / 3}px`,
    },
}));

const DateCard = styled('div')(({ theme }) => ({
    padding: '5px 10px',
    borderRadius: '5px',
    position: 'absolute',
    top: '10px',
    right: '10px',
    fontSize: '0.9rem',
}));

const CardContent = styled('div')({
    display: 'flex',
    flexDirection: 'column',
    gap: '10px',
});

const Card = ({ data }) => {
    const theme = useTheme();
    return (
        <CardContainer theme={theme}>
            <StyledCard theme={theme}>
                <DateCard>
                    {"date"}
                </DateCard>
                <CardContent>
                    <h3>{data.description}</h3>
                    <p>{data.text}</p>
                </CardContent>
            </StyledCard>
        </CardContainer>
    );
};

export default Card;
