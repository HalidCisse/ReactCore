import axios     from 'axios'
import { Modal } from 'antd'

let login_warning = null

export const http = axios.create({ baseURL: window.API })

http.interceptors.request.use(config=> {
    if (config.url.includes(window.API)) {
        config.headers.common['Authorization'] = localStorage.Authorization
    }
    return config
}, error => {
    return Promise.reject(error)
})

http.interceptors.response.use(response=> response, error =>{
    if(error)
        console.error(error)

    if (error && error.response && 401 === error.response.status) {
        console.log('401 redirect to login')

        if(login_warning || window.location.pathname.includes('login') || window.location.pathname.includes('sharing'))
            return
        login_warning = Modal.warning({
            title: 'Please sign in again',
            content: 'You were signed out of your account.',
            okText: 'Sign in',
            onOk() {
                login_warning.destroy()
                login_warning = null
                setTimeout(() => {

                    if(window.appHistory){
                        window.appHistory.push(`/login?returnUrl=${window.location.pathname}`)
                        return window.appHistory.go()
                    }

                    history.push(`/login?returnUrl=${window.location.pathname}`)
                    window.location.href = window.location.href
                    setTimeout(() => {
                        if(!window.location.pathname.includes('login'))
                        {
                            console.log('location.reload()')
                            window.location.reload()
                        }else {
                            console.log('No location.reload()')
                        }
                    }, 2)
                }, 2)
            }
        })
    } else {
        return Promise.reject(error)
    }
})