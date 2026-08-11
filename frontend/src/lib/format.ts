const currencyFormatter = new Intl.NumberFormat("en-GB", {
  style: "currency",
  currency: "GBP",
});

function formatPrice(pence: number): string {
  return currencyFormatter.format(pence / 100);
}

export { formatPrice };
