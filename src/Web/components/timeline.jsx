import React from 'react';
import PropTypes from 'prop-types';
import TimelineItem from './timelineitem.jsx'

const propTypes = {
    items: PropTypes.array,
    openModal: PropTypes.func
}

export default class Timeline extends React.Component {
    
    static get defaultProps() {
        return  {
            items: [],
            openModal: () => {}
        }
    }

    constructor(props){
        super(props);
    }

    render() {
        return (
            <ul className="timeline">
                {this.props.items.map((item, i) => (
                    <TimelineItem inverted={!(i % 2 == 0)} item={item} key={i} openModal={this.props.openModal}  />
                ))}           
            </ul>
        );
    }
}
Timeline.propTypes = propTypes;