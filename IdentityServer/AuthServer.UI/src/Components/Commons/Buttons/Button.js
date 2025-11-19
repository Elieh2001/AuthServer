import React from 'react';
import PropTypes from 'prop-types';
import './Button.css';

const Button = ({ text, onClick, styleClass }) => {
    return (
        <button className={`button ${styleClass}`} onClick={onClick}>
            {text}
        </button>
    );
};

Button.propTypes = {
    text: PropTypes.string.isRequired,
    onClick: PropTypes.func,
    styleClass: PropTypes.string
};


export default Button;
