# ProductBasket represents actual class that can recieve messages and apply them 
class ProductBasket
  constructor: ->
    @_products = {}

  addProductToBasket: (name, quantity) ->
    if @_products[name]?
      @_products[name] += quantity
    else
      @_products[name] = quantity

  removeProductFromBasket: (name, quantity) ->
    if @_products[name]?
      @_products[name] -= quantity
      if @_products[name] <= 0
        delete @_products[name]
      console.log "#{quantity} #{name}'s removed succesfully"
    else
      console.log "you don't have #{name} in your basket"

  when: (message) ->
    if message.msgName is 'AddProductToBasketMessage'
      @addProductToBasket message.name, message.quantity
    if message.msgName is 'RemoveProductFromBasketMessage'
      @removeProductFromBasket message.name, message.quantity



# AddProductToBasketMessage represents message 
# which has name and some properties and add items to basket
class AddProductToBasketMessage
  constructor: (@name, @quantity) ->
    @msgName = 'AddProductToBasketMessage'



# RemoveProductToBasketMessage represents message 
# which has name and some properties and removes items from basket
class RemoveProductFromBasketMessage
  constructor: (@name, @quantity) ->
    @msgName= 'RemoveProductFromBasketMessage'


#queue
class Queue
  constructor: ->
    @_items = []

  enqueue: (obj) ->
    if not @_items[obj]?
      @_items.push obj

  dequeue: ->
    @_items.shift()  

  isEmpti: ->
    @_items.length is 0

# add some products to basket and then remove them
message = new AddProductToBasketMessage 'Banana', 3
basket  = new ProductBasket()
basket.when message

message = new AddProductToBasketMessage 'Strawberry', 1
basket.when message

message = new RemoveProductFromBasketMessage 'Banana', 2
basket.when message

console.log basket


#test serilization
#read messages from file and apply them to basket object
fs = require 'fs'
queue = new Queue()
fs.readFile 'messages.json', (err,messages) ->
  messages = JSON.parse messages.toString()
  for msg in messages
    queue.enqueue msg

  while not queue.isEmpti()
    msg = queue.dequeue()
    basket.when msg

  console.log basket
