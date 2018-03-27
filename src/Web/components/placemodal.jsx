import React from 'react';
import PropTypes from 'prop-types';
import Modal from 'react-bootstrap-modal'
import Carousel from 'react-bootstrap/lib/Carousel'

const propTypes = {
    open: PropTypes.bool,
        
    closeModal: PropTypes.func
}

export default class PlaceModal extends React.Component {
    
    static get defaultProps() {
        return {
            open: false,
            item: null,
            closeModal: () => {}
        }
        
    }

    constructor(props){
        super(props);

        this.state = {
            open: this.props.open
        }
    }

    render() {  
        return (  
            <div> 
                {this.props.item != null ?                     
                    <Modal show={this.props.open} onHide={() => this.props.closeModal()} aria-labelledby="ModalHeader">                
                        <Modal.Header closeButton>
                            <Modal.Title id='ModalHeader'>{this.props.item.Name}</Modal.Title>
                        </Modal.Header>
                        <Modal.Body>
                            <div>
                            <Carousel>
                            {this.props.item.Photos.map((item, i) => (
                                <Carousel.Item key={i}>
                                    <img className="placeImage" src={item.Url} />
                                    <Carousel.Caption>
                                        <div className="carousel-caption-text">                                    
                                            {item.Captions.map((caption, i) => (
                                                <h3 key={i}>"{caption}"</h3>
                                            ))}   

                                            <p><strong>Categories</strong></p> 
                                            <div className="place-modal-photos-tags">
                                                {item.Categories.map((category, i) => (
                                                    <i key={i}>{category}, </i> 
                                                ))}   
                                            </div>  
                                            <p><strong>Tags</strong></p>
                                            <div className="place-modal-photos-tags">
                                                {item.Tags.map((tag, i) => (
                                                    <i key={i}>{tag}, </i> 
                                                ))}   
                                            </div> 
                                        </div>
                                    </Carousel.Caption>
                                </Carousel.Item>
                            ))} 
                            </Carousel> 
                            </div>
                        </Modal.Body>
                        <Modal.Footer>
                            <Modal.Dismiss className='btn btn-default'>Close</Modal.Dismiss>   
                        </Modal.Footer>                
                    </Modal>  
                : null }
            </div>                   
        );
    }
}
PlaceModal.propTypes = propTypes;