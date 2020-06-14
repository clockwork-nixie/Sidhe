import Gui from './gui.js';

class LoginForm {
    constructor(document, onLogin) {
        this._onLogin = onLogin;

        this._loginPage = document.getElementById('loginPage');
        this._loginButton = document.getElementById('login');
        this._passwordField = document.getElementById('password');
        this._usernameField = document.getElementById('username');

        const self = this;

        this._loginButton.addEventListener('click', () => self.login());

        this._passwordField.addEventListener('keyup', () => self.setFormState());
        this._usernameField.addEventListener('keyup', () => self.setFormState());

        this._passwordField.addEventListener('keydown', (event) => self.onKeyDown(event));
        this._usernameField.addEventListener('keydown', (event) => self.onKeyDown(event));
        
        this._passwordField.addEventListener('paste', () => window.setTimeout(() => self.setFormState(), 0));
        this._passwordField.addEventListener('cut', () => window.setTimeout(() => self.setFormState(), 0));
        this._usernameField.addEventListener('paste', () => window.setTimeout(() => self.setFormState(), 0));
        this._usernameField.addEventListener('cut', () => window.setTimeout(() => self.setFormState(), 0));
    }


    hide() { this._loginPage.classList.add('hidden'); }


    login() {
        const password = this._passwordField.value;
        const username = this._usernameField.value;

        if (username && password && this._onLogin) {
            try {
                this._loginButton.disabled = true;
                this._onLogin(username, password);
            } catch (error) {
                alert(error.message);
            } finally {
                this._loginButton.disabled = false;
            }
        }
    }


    onKeyDown(event) {
        if (event.key === 'Enter') {
            if (event.target === this._usernameField && this._usernameField.value) {
                this._passwordField.focus();
            } else if (event.target === this._passwordField && this._usernameField.value) {
                if (this._usernameField.value) {
                    this.login();
                } else {
                    this._usernameField.focus();
                }
            }
        }
    }


    reset() {
        this._passwordField.value = '';
    }


    setFormState() { this._loginButton.disabled = !(this._passwordField.value && this._usernameField.value); }


    show() {
        this._loginPage.classList.remove('hidden'); 
        this.setFormState();

        if (this._usernameField.value) {
            this._passwordField.focus();
        } else {
            this._usernameField.focus();
        }
    }
}


class MainForm {
    constructor(document, comms, onLogout) {
        this._gui = null;
        this._comms = comms;
        this._container = document.getElementById('phaser');
        this._mainPage = document.getElementById('mainPage');

        var logoutButton = document.getElementById('logout');
  
        logoutButton.addEventListener('click', () => onLogout('User request.'));
    }


    hide() {
        this._mainPage.classList.add('hidden');

        if (this._gui) {
            this._gui.destroy();
            this._gui = null;
        }
    }


    received(data) {
        console.log(`COMMAND(${JSON.stringify(data)})`);

        if (this._gui) {
            this._gui.received(data);
        }
    }


    show() {
        if (!this._gui) {
            this._gui = new Gui(this._container);
            this._gui.sendUpdate(this._comms);
        }
        this._mainPage.classList.remove('hidden');
    }
}


export default class Ui {
    constructor(document, comms) {
        this._comms = comms;
        this._state = 'LOGIN';
        this.onPosition = null;

        const self = this;

        const performLogin = (async (username, password) => {
            if (await comms.login(username, password)) {
                self._state = 'CONNECTING';
            } else {
                alert('Login Failed');
            }
        });

        this._loginForm = new LoginForm(document, performLogin);
        this._mainForm = new MainForm(document, comms, (message) => self.logout(message));

        this._loginForm.show();       
    }


    logout(reason) {
        this._comms.logout(reason);
        this._mainForm.hide();
        this._loginForm.reset();
        this._loginForm.show();
        this._state = 'LOGIN';
    }
    

    routeEvent(eventName, data) {
        if (eventName === 'close') {
            if (this._state !== 'LOGIN') {
                alert('Connection closed.');
            }
            this.logout('Connection closed.');
        } else {
            console.log(`WebSocket event: ${eventName}: ${JSON.stringify(data)}`);
        }
    }
    
    routeMessage(message) {
        if (!message) {
            console.log('UI!! Empty message.');
        } else if (message[0] === '{' && message[message.length - 1] === '}') {
            try {
                if (this.onPosition) {
                    this.onPosition(message);
                } else {
                    this._mainForm.received(JSON.parse(message));
                }
            } catch (error) {
                console.log(`UI!! ERROR: ${error.message}`)
            }
        } else if (message === 'QUIT') {
            if (this._state !== 'LOGIN') {
                alert('Connection closed.');
                this.logout('Connection closed.');
            } else {
                console.log(`UI!! Command ${message} in state ${this._state}`);
            }
        } else if (message === 'OPEN') {
            if (this._state === 'CONNECTING') {
                this._state = 'CONNECTED';
                this._loginForm.hide();
                this._mainForm.show();
            } else {
                console.log(`UI!! Command ${message} in state ${this._state}`);
            }
        } else {
            console.log(`UI!! Unknown message ${message}`);
        }
     }
}