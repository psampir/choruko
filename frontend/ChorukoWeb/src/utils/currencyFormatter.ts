// Format currency amounts with proper symbol placement based on symbol length
// Convention: Single-character symbols (e.g., $, €, £) go before the amount
//            Multi-character symbols (e.g., zł, kr, Ft) go after with a space
export const formatCurrency = (
  amount: number,
  currencySymbol: string
): string => {
  const formattedAmount = amount.toFixed(2);
  
  // Single character symbols go before (e.g., $50.00, €50.00, £50.00)
  if (currencySymbol.length === 1) {
    return `${currencySymbol}${formattedAmount}`;
  }
  
  // Multi-character symbols go after with space (e.g., 50.00 zł, 50.00 kr)
  return `${formattedAmount} ${currencySymbol}`;
};
