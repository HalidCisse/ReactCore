import React, { Component } from 'react'
import API from "./API"

import './App.scss'

export default class App extends Component {
  
  constructor(){
    super()
  }
  
  componentDidMount() {
    API
        .files
        .get()
        .then(res=>{
          
          console.log(res.data)
        })
        .catch(err=>{
          console.error(err)
        })
  }

  render() {
    return (
      <div className="App">
        <header className="App-header">
          .NET Core + Electron = ♥
        </header>
        
        <div className='fs'>
          <div>
            <header>
              Here is your drives
            </header>

            <div>

            </div>
          </div>

          <div>
            <header>
              Here is your files
            </header>

            <div>

            </div>
          </div>
        </div>
      </div>
    )
  }
}