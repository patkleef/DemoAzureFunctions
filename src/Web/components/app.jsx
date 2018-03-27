import React from 'react';
import * as ReactDOM from 'react-dom';
import PropTypes from 'prop-types';
import Timeline from './timeline.jsx'
import PlaceModal from './placemodal.jsx'

const propTypes = {
    items: PropTypes.array,
}

export default class App extends React.Component {

    constructor(props){
        super(props);
        this.state= {
            open: false, 
            item: null
        }
    }

    openModal(item) {
        this.setState({
            open: true,
            item: item
        })
    }

    closeModal() {
        this.setState({
            open: false,
            item: null
        })
    }

    render() {
        return (
            <div>
                <Timeline items={this.props.items} openModal={this.openModal.bind(this)}  />
                <PlaceModal open={this.state.open} closeModal={this.closeModal.bind(this)} item={this.state.item}  />
            </div>
        );
    }
}
App.propTypes = propTypes;

window.fetch('https://demoazurefunctions1.azurewebsites.net/api/WebDataFunction?code=CI/uFG/9u9a3gDPEh22qrVRIEoqgeF44SLxk6Ep9JIAg/D6bdVrqkA==', {
    method: 'get'
}).then(function(response) {
    response.json().then(function(data) {

        ReactDOM.render(<App items={data} />, document.getElementById('app'));
        });
    
});