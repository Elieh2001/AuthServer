import React from 'react';
import PropTypes from 'prop-types';
import { styled } from '@mui/material/styles';

const TextBoxContainer = styled('div')(({ theme }) => ({
    position: 'relative',
    marginTop: '15px',
    display: 'inline-block',
    width: '100%',
}));

const StyledInput = styled('input')(({ theme }) => ({
    padding: '10px 16px',
    cursor: 'pointer',
    backgroundColor: theme.palette.primary.main,
    color: theme.palette.text.primary,
    border: `1px solid ${theme.palette.divider}`,
    borderRadius: '3px',
    display: 'flex',
    alignItems: 'center',
    flexDirection: 'row',
    width: '100%',
    boxSizing: 'border-box',
    '&:focus': {
        outline: 'none',
        borderColor: theme.palette.text.primary,
    },
}));

const TextBoxLabel = styled('label')(({ theme }) => ({
    position: 'absolute',
    left: '16px',
    top: '50%',
    transform: 'translateY(-50%)',
    background: theme.palette.primary.main,
    padding: '0 4px',
    transition: '0.2s',
    pointerEvents: 'none',
    color: '#999',
    '.textbox:focus + &': {
        top: 0,
        left: '12px',
        color: '#666',
        fontSize: '12px',
        transform: 'translateY(-50%) scale(0.8)',
    },
    '.textbox:not(:placeholder-shown) + &': {
        top: 0,
        left: '12px',
        color: '#666',
        fontSize: '12px',
        transform: 'translateY(-50%) scale(0.8)',
    },
}));

const TextBox = ({ placeholder, type }) => {

    return (
        <TextBoxContainer>
            <StyledInput
                type={type}
                placeholder=" "
                required
                className="textbox"
            />
            <TextBoxLabel className="textbox-label">{placeholder}</TextBoxLabel>
        </TextBoxContainer>
    );
};

TextBox.propTypes = {
    placeholder: PropTypes.string.isRequired,
    type: PropTypes.string.isRequired,
};

export default TextBox;
