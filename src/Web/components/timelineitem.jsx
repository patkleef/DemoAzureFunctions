import React from 'react';
import PropTypes from 'prop-types';
import ReactGoogleMapLoader from "react-google-maps-loader"
import ReactGoogleMap from "react-google-map"

const propTypes = {
    inverted: PropTypes.bool,
    item: PropTypes.object,
    openModal: PropTypes.func
}

export default class TimelineItem extends React.Component {
    
    static get defaultProps() {
        return  {
            inverted: false,
            item: null,
            openModal: () => {},
        }        
    }

    constructor(props){
        super(props);
    }

    render() {
        return (
            <li className={this.props.inverted ? "timeline-inverted" : ""}>
                <div className="timeline-badge">
                    <span className="badge-time">20:13</span>
                </div>
                <div className="timeline-panel">
                    <div className="timeline-heading">
                        <h2 className="timeline-title">{this.props.item.Address}</h2>
                        <p><small className="text-muted"><i className="glyphicon glyphicon-map-marker"></i> {this.props.item.Name}</small></p>
                    </div>
                    <div className="timeline-body">
                    
                    {this.props.item.Coordinates !== null ?
                        <ReactGoogleMapLoader
                            params={{
                                key: "AIzaSyBCNgdFABzZdMzSY3Ynt1hugVAwf8lZR34", 
                                libraries: "places,geometry", 
                            }}
                            render={googleMaps => 
                                googleMaps && (
                                    <div style={{height: "300px"}}>
                                        <ReactGoogleMap
                                        googleMaps={googleMaps}
                                        coordinates={[
                                            {
                                            title: this.props.item.Address,
                                            position: {
                                                lat: this.props.item.Coordinates.Latitude,
                                                lng: this.props.item.Coordinates.Longitude,
                                            },
                                            onLoaded: (googleMaps, map, marker) => {
                                                marker.setIcon("/images/marker.png")
                                            }
                                        }]}
                                        center={{lat: this.props.item.Coordinates.Latitude, lng: this.props.item.Coordinates.Longitude}}
                                        zoom={14}
                                        />
                                    </div>
                            )}
                        />
                    : null }
                    </div>
                    <div>
                        <h3>Nearby Places</h3>
                        <ul className="timeline-places">
                            {this.props.item.Places.map((item, i) => (
                                <li key={i}>
                                    <p><strong>{item.Name}</strong></p>
                                    <img src={item.Photos[1].Url} className="timeline-place-image" />   
                                    <div className="timeline-place-types">
                                        {item.Types.map((type, j) => (
                                            <span key={j}><i>{type}</i> | </span>
                                        ))}       
                                    </div> 
                                    <a href="#" onClick={() => this.props.openModal(item)}>More</a>                               
                                </li>
                            ))}    
                        </ul>
                    </div>
                </div>
            </li>
        );
    }
}
TimelineItem.propTypes = propTypes;