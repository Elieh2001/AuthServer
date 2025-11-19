import React, { useState, useRef, useEffect } from 'react';
import { styled } from '@mui/material/styles';
import { useTheme } from '@mui/material/styles';
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import DialogContent from '@mui/material/DialogContent';
import DialogActions from '@mui/material/DialogActions';
import { IconButton, Slide } from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';

const CardContainer = styled('div')(({ theme }) => ({
    display: 'inline-flex',
    width: "50%",
    margin: "0px",
    gap: '20px',
    [theme.breakpoints.down('md')]: {
        borderRadius: "0px",        
        width: '100%',
    },
    [theme.breakpoints.up('md')]: {
        height: "30%",
    },
}));

const DateCard = styled('div')(({ theme }) => ({
    color: theme.palette.date.primary,    
    position: 'absolute',
    right: '5px',
    fontSize: '0.7rem',
    paddingRight:"8px",
    // backgroundColor: theme.palette.background.paper,
    // Desktop view 
    [theme.breakpoints.up('md')]: {        
        backgroundColor: 'transparent',
        color: theme.palette.text.primary,    
    },
}));

const CardContent = styled('div')({
    display: 'flex',
    flexDirection: 'column',    
    flex: 1,
});


const Title = styled('h3')(({ theme }) => ({    
    fontWeight: "bold",
    overflow: 'hidden',
    textOverflow: 'ellipsis',
    whiteSpace: 'nowrap', 
    margin: 0,
    flex: 1,
    width: '70%',
    [theme.breakpoints.down('md')]: {
    width:"80%"
    }
}));


const StyledCard = styled('div')(({ theme }) => ({
    backgroundColor: theme.palette.primary.main,
    color: theme.palette.text.primary,
    width: '100%',
    margin: "0px",
    paddingBottom:"5px", 
    paddingTop:"0px",
    position: 'relative',
    display: 'flex',
    flexDirection: 'column',
    gap: '10px',
    boxSizing: 'border-box',
    overflow: 'hidden',
    cursor: 'pointer',
    [theme.breakpoints.up('md')]: {
        boxShadow: '0 4px 8px rgba(0, 0, 0, 0.1)',
        margin: '20px',
        borderRadius: '8px',
    },
    [theme.breakpoints.down('md')]: {        
        borderTop: `1px solid ${theme.palette.divider}`,
    },
}));

const BodyText = styled('p')(({ theme, lineClamp }) => ({
    display: '-webkit-box',
    padding:"0px 8px 5px 8px",    
    WebkitLineClamp: 2,
    WebkitBoxOrient: 'vertical',
    overflow: 'hidden',
    textOverflow: 'ellipsis',
    margin: 0,
    height:"51px",
    boxSizing: 'border-box',
    wordWrap: 'break-word',  // Allows long words to break
    overflowWrap: 'break-word',  // Similar to word-wrap but more compatible
    wordBreak: 'break-word',  // Forces long words to break
    // Desktop view
    [theme.breakpoints.up('md')]: {
        flex: 1,
        WebkitLineClamp: lineClamp,
        display: '-webkit-box',
        WebkitBoxOrient: 'vertical',
        overflow: 'hidden',
        textOverflow: 'ellipsis',
        position: 'relative',
    },
}));

const TitleContainer = styled('div')(({ headColor }) => ({
    display: 'inline-flex',    
    paddingBottom:"5px",
    width: '100%',
    margin: 0,
    flexDirection: 'column', 
    backgroundColor: headColor,
    paddingLeft:"8px",
}));

const Card = ({ id, title, body, rightItem, headerColor, isMobile }) => {
    const theme = useTheme();
    const [open, setOpen] = useState(false);
    const [dialogStyle, setDialogStyle] = useState({});
    const [lineClamp, setLineClamp] = useState(2);    
    const dateCardRef = useRef(null);
    const cardRef = useRef(null);

    // Use headerColor directly
    const headColor = headerColor();

    useEffect(() => {
        if (!isMobile && cardRef.current) {
            const containerHeight = cardRef.current.clientHeight;
            const lineHeight = parseFloat(getComputedStyle(cardRef.current).lineHeight);
            const calculatedLineClamp = Math.floor(containerHeight / lineHeight) - 1; 
            setLineClamp(calculatedLineClamp);
        }
    }, [isMobile, cardRef.current?.clientHeight]);

    const handleClick = () => {
        setDialogStyle({
            width: `100vw`,
            height: `100vh`,
        });
        setOpen(true);
    };

    const handleClose = () => {
        setOpen(false);
    };

    return (
        <>
            <CardContainer theme={theme}  id={id}>
                <StyledCard theme={theme} ref={cardRef}>               
                    <CardContent onClick={handleClick}>
                        <TitleContainer headColor={headColor}> 
                            {rightItem && <DateCard ref={dateCardRef} theme={theme}>{rightItem}</DateCard>}                            
                            <Title>{title}</Title>
                        </TitleContainer>
                        <BodyText lineClamp={lineClamp}>{body}</BodyText>                        
                    </CardContent>
                </StyledCard>
            </CardContainer>
            
            <Dialog 
                fullScreen
                open={open} 
                onClose={handleClose}
                PaperProps={{
                    style: dialogStyle,
                }}
                TransitionComponent={Slide}
                TransitionProps={{
                    direction: open ? 'left' : 'left',
                }}
            >                
                <DialogTitle>{title}</DialogTitle>
                <DialogContent>                    
                    <p>{body}</p>
                </DialogContent>
                <DialogActions sx={{ position: 'absolute', top: 30, right: 0 }}>
                    <IconButton 
                        sx={{ 
                            borderRadius: '6px',
                            mr: theme.spacing(1.25),
                            color: 'inherit',
                            backgroundColor: theme.palette.buttonsHover.light,
                            '&:hover': {
                                backgroundColor: theme.palette.buttonsHover.dark, 
                            },  
                        }} 
                        onClick={handleClose} 
                    >
                        <CloseIcon   
                            sx={{
                                color: theme.palette.text.primary,
                                fontSize: '20px',
                            }}
                        />
                    </IconButton>
                </DialogActions>
            </Dialog>
        </>
    );
};

export default Card;
