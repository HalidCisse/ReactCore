import React, { Component } from 'react'
import API from "./API"

import './App.scss'

export default class App extends Component {
  
  constructor(){
    super()
    
    this.state ={
      drives:[],
      files:[]
    }
  }
  
  componentDidMount() {
    API
        .files
        .drives()
        .then(res=>{
          this.setState({drives:res.data})
          console.log(res.data)
        })
        .catch(err=>{
          console.error(err)
        })
    
    API
        .files
        .get()
        .then(res=>{
          this.setState({files:res.data})
          console.log(res.data)
        })
        .catch(err=>{
          console.error(err)
        })
  }

  render() {
    
    const {drives, files} = this.state
    
    return (
      <div className="App">
        <header className="App-header">
          .NET Core + Electron = ♥
        </header>
        
        <div className='fs'>
          <div>
            <h2>Drives</h2>

            <div>
              {
                drives.map(d=>{
                  return <a className='drive'>
                        Name: {d.name} - ({d.totalSize})
                  </a>
                })
              }
            </div>
          </div>

          <div>
            <h2>Files - <a href='/fs'>Browse files</a></h2>

            <div>
              <ul>
                {
                  files.map(item=>{
                    if (item.IsDirectory)
                    {
                      return <ul>
                        <li><strong>{item.name}</strong></li>
                      </ul>
                    }
                    else
                    {
                      return <li>{item.name} - {item.length} bytes</li>
                    }
                  })
                }
              </ul>
            </div>
          </div>
        </div>
      </div>
    )
  }
}