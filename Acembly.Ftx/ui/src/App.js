import React, { Component } from 'react'
import logo from './logo.svg'
import './App.css'

export default class App extends Component {
  
  constructor(){
    super()
  }
  
  componentDidMount() {
    // fetch('api/files')
    //     .then(response => response.json())
    //     .then(data => {
    //       this.setState({ forecasts: data, loading: false });
    //     })
  }

  render() {
    return (
      <div className="App">
        <header className="App-header">
          <img src={logo} className="App-logo" alt="logo" />
          <p>
            Edit <code>src/App.js</code> and save to reload.
          </p>
          <a
            className="App-link"
            href="#"
            target="_blank"
            rel="noopener noreferrer">
            Ftx Demo
          </a>
        </header>
      </div>
    );
  }
}