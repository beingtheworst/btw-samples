package com.beingtheworst.e002;

import java.io.ByteArrayOutputStream;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.ObjectInputStream;
import java.io.ObjectOutputStream;
import java.io.Serializable;
import java.util.HashMap;
import java.util.LinkedList;
import java.util.Map;
import java.util.Map.Entry;
import java.util.Queue;

public class Program {

	public static void main(String[] args) throws Exception {
		ProductBasket basket = new ProductBasket();
		
		basket.addProduct("butter", 1);
		
		basket.addProduct("pepper", 2);
		
		AddProductToBasketMessage message = new AddProductToBasketMessage("candles", 5);
		
		applyMessage(basket, message);
		
		Queue<Object> queue = new LinkedList<Object>();
		queue.offer(new AddProductToBasketMessage("Chablis wine", 1));
		queue.offer(new AddProductToBasketMessage("shrimps", 10));
		
		for (Object item: queue) {
			System.out.println(" [Message in Queue is:] * " + item);
		}
		
		while (!queue.isEmpty()) {
			applyMessage(basket, queue.poll());
		}
		
		AddProductToBasketMessage msg = new AddProductToBasketMessage("rosemary", 1);

		ByteArrayOutputStream baos = new ByteArrayOutputStream();
		ObjectOutputStream oos = new ObjectOutputStream(baos);
		oos.writeObject(msg);
		byte[] bytes = baos.toByteArray();

		FileOutputStream fos = new FileOutputStream("message.bin");
		fos.write(bytes);
		fos.close();
		
		FileInputStream fis = new FileInputStream("message.bin");
		ObjectInputStream ois = new ObjectInputStream(fis);
		applyMessage(basket, ois.readObject());
		ois.close();
		
		for (Entry<String, Integer> entry: basket.getProductTotals().entrySet()) {
			System.out.println(String.format("  %s: %d", entry.getKey(), entry.getValue()));
		}
	}

	private static void applyMessage(ProductBasket basket, Object message) {
    	basket.when(message);
	}
	
	public static class ProductBasket {
		Map<String, Integer> products = new HashMap<String, Integer>();
		
		public void addProduct(String name, Integer quantity) {
			Integer currentQuantity = 0;
			if (products.containsKey(name)) {
				currentQuantity = products.get(name);
			}
			products.put(name, currentQuantity + quantity);
			System.out.println(String.format("Shopping Basket said: I added %d unit(s) of %s", quantity, name));
		}
		
		private void when(AddProductToBasketMessage message) {
			System.out.print("[Message Applied]: ");
			message.apply(this);
		}
		
		public void when(Object message) {
			if (message instanceof AddProductToBasketMessage) {
				when((AddProductToBasketMessage)message);
			}
		}

		public Map<String, Integer> getProductTotals() {
			return products;
		}
	}
	
	public static class AddProductToBasketMessage implements Serializable {
		private static final long serialVersionUID = -7100184937020649619L;

		public String name;
		public Integer quantity;
		
		public AddProductToBasketMessage(String name, Integer quantity) {
			this.name = name;
			this.quantity = quantity;
		}

		public void apply(ProductBasket basket) {
			basket.addProduct(name, quantity);
		}
		
		public String toString() {
			return String.format("Add %d %s to basket", quantity, name);
		}
	}

}
