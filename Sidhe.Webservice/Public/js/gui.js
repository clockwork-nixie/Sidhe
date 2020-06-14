function phaserPreload(game) {
    game.load.image('sky', 'assets/sky.png');
    game.load.image('star', 'assets/star.png');
    game.load.image('bomb', 'assets/bomb.png');
}


function phaserCreate(game, state) {
    game.add.image(400, 300, 'sky');

    state.keyboard = game.input.keyboard.addKeys({
        up: "W",
        down: "S",
        left: "A",
        right: "D"
    });
    state.physics = game.physics;
    state.sprite = game.physics.add.image(state.x, state.y, 'star');

    state.sprite.setCollideWorldBounds(true);

    state.isCreated = true;
}


function phaserUpdate(game, state) {
    const keyboard = state.keyboard;

    if (keyboard.left.isDown && !keyboard.right.isDown) {
        state.sprite.setVelocityX(-160);
    } else if (keyboard.right.isDown && !keyboard.left.isDown) {
        state.sprite.setVelocityX(160);
    } else {
        state.sprite.setVelocityX(0);
    }

    if (keyboard.up.isDown && !keyboard.down.isDown) {
        state.sprite.setVelocityY(-160);
    } else if (keyboard.down.isDown && !keyboard.up.isDown) {
        state.sprite.setVelocityY(160);
    } else {
        state.sprite.setVelocityY(0);
    }
    state.x = Math.floor(state.sprite.x);
    state.y = Math.floor(state.sprite.y);
}


export default class Gui {
    constructor(parent) {
        const self = this;
        
        this._isValid = false;

        this._state = {
            cursors: null,
            isCreated: false,
            physics: null,
            sprite: null,
            others: {},
            x: Math.floor(Phaser.Math.FloatBetween(20, 780)),
            y: Math.floor(Phaser.Math.FloatBetween(20, 380))
        };

        this._game = new Phaser.Game({
            parent: parent,
            type: Phaser.AUTO,
            width: 800,
            height: 600,
            physics: {
                default: 'arcade',
                arcade: { debug: false }
            },
            scene: {
                preload: function() { phaserPreload(this); },
                create: function() { phaserCreate(this, self._state); },
                update: function() { phaserUpdate(this, self._state); }
            }
        });

        this._isValid = true;
    }


    destroy() {
        this._isValid = false;

        if (this._game) {
            const game = this._game;

            this._game = null;
            game.destroy(true);
        }
    }
   

    received(message) {
        try {   
            if (message.changes) {
                const positions = message.changes;
                const state = this._state;
    
                for (let i = 0; i < positions.length; ++i) {
                    const position = positions[i];
                    const sprite = state.others[position.userId];
                    
                    if (sprite) {
                        sprite.x = position.x;
                        sprite.y = position.y;
                    } else {
                        state.others[position.userId] = state.physics.add.image(position.x, position.y, 'bomb');
                    }
                }
            }
        } catch (error) {
            console.log(error.message);
        }
    }    
    

    sendUpdate(comms) {       
        try {
            if (this._isValid && this._state.isCreated) {
                comms.send(JSON.stringify({ 
                    location: {
                        x: this._state.x,
                        y: this._state.y
                    }
                }));
            }
        } finally {
            const self = this;
            window.setTimeout(function() { self.sendUpdate(comms); }, 10);
        }
    }
}