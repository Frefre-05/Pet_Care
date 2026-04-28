# def = repeating a function
# str = "text"
# bool = T or F
# input = ask whatever you want the user to put
# int = number
# float = number with decimal
# f strings = text that you can put {} so that whatever is inside, the actual variable will be called
# for = also a loop but not a while loop
# while = main loop used

def celsius_to_farenheight(celsius):
    farenheight = (celsius + 20) / 2
    return farenheight

print(celsius_to_farenheight(100))

def cost_of_pizza(price):
    final_price = (price * 1.5)
    return final_price

print(cost_of_pizza(76.86))