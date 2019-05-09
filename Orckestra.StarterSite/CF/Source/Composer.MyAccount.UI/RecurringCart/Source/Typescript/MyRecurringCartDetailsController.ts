///<reference path='../../../../Composer.UI/Source/Typings/tsd.d.ts' />
///<reference path='../../../../Composer.UI/Source/TypeScript/Mvc/Controller.ts' />
///<reference path='../../../../Composer.UI/Source/TypeScript/Mvc/IControllerActionContext.ts' />
///<reference path='./RecurringCartDetailsController.ts' />
///<reference path='../../../../Composer.Cart.UI/RecurringOrder/source/TypeScript/Services/RecurringOrderService.ts' />
///<reference path='../../../../Composer.Cart.UI/RecurringOrder/source/TypeScript/Repositories/RecurringOrderRepository.ts' />


module Orckestra.Composer {

    enum EditSection {
        NextOccurence = 0,
        ShippingMethod = 1
    };

    export class MyRecurringCartDetailsController extends Orckestra.Composer.RecurringCartDetailsController {
        private recurringOrderService: IRecurringOrderService = new RecurringOrderService(new RecurringOrderRepository(), this.eventHub);
        private editNextOcurrence = false;
        private editShippingMethod = false;
        private originalShippingMethodType = '';
        private hasShippingMethodTypeChanged = false ;

        public initialize() {

            super.initialize();

            console.log(this.context.viewModel);
            //this.originalShippingMethodType = this.context.viewModel.ShippingMethodFulfillmentMethodType;
        }

        public toggleEditNextOccurence(actionContext: IControllerActionContext) {
            var context: JQuery = actionContext.elementContext;

            this.editNextOcurrence = !this.editNextOcurrence;

            if (this.editNextOcurrence) {
                this.closeOtherEditSections(actionContext, EditSection.NextOccurence);
            }

            let nextOccurence = context.data('next-occurence');
            let formatedNextOccurence = context.data('formated-next-occurence');
            let nextOccurenceValue = context.data('next-occurence-value');
            let total = context.data('total');

            let vm = {
                EditMode: this.editNextOcurrence,
                NextOccurence: nextOccurence,
                FormatedNextOccurence: formatedNextOccurence,
                NextOccurenceValue: nextOccurenceValue,
                OrderSummary: {
                    Total: total
                }
            };

            this.render('RecurringCartDetailsSummary', vm);
        }

        public saveEditNextOccurence(actionContext: IControllerActionContext) {
            var context: JQuery = actionContext.elementContext;

            let element = <HTMLInputElement>$('#NextOcurrence')[0];
            let newDate = element.value;
            let isValid = this.nextOcurrenceIsValid(newDate);

            if (isValid) {

                let cartName = this.context.viewModel.Name;

                let data: IRecurringOrderLineItemsUpdateDateParam = {
                    CartName: cartName,
                    NextOccurence: newDate
                };

                this.recurringOrderService.updateLineItemsDate(data)
                    .then((viewModel) => {

                        let hasMerged = viewModel.RescheduledCartHasMerged;

                        if (hasMerged) {
                            //Redirect to my orders
                            //TODO
                        } else if (!_.isEmpty(viewModel)) {
                            //Render TODO
                            //this.render('MyRecurringCarts', viewModel);
                        }

                        //busyHandle.done();
                    })
                    .fail((reason) => {
                        console.error(reason);
                    //  busyHandle.done();
                    });
            } else {
                console.log('Error: invalid date');
            }
        }

        public nextOcurrenceIsValid(value) {
            let newDate = this.convertDateToUTC(new Date(value));
            let today = this.convertDateToUTC(new Date(new Date().setHours(0, 0, 0, 0)));

            if (newDate > today) {
                return true;
            }
            return false;
        }

        private convertDateToUTC(date) {
            return new Date(date.getUTCFullYear(), date.getUTCMonth(), date.getUTCDate(),
            date.getUTCHours(), date.getUTCMinutes(), date.getUTCSeconds());
        }

        public toggleEditShippingMethod (actionContext: IControllerActionContext) {
            var context: JQuery = actionContext.elementContext;

            this.editShippingMethod = !this.editShippingMethod;

            let shippingMethodDisplayName = context.data('shipping-method-display-name');
            let shippingMethodCost = context.data('shipping-method-cost');
            let shippingMethodName = context.data('selected-shipping-method-name');
            let shippingMethodFulfillmentType = context.data('selected-shipping-method-fulfillment-type');

            //TODO
            this.originalShippingMethodType = shippingMethodFulfillmentType;

            if (this.editShippingMethod) {
                this.closeOtherEditSections(actionContext, EditSection.ShippingMethod);

                this.getShippingMethods(this.context.viewModel.Name)
                    .then(shippingMethods => {

                        if (!shippingMethods) {
                            throw new Error('No viewModel received');
                        }

                        if (_.isEmpty(shippingMethods.ShippingMethods)) {
                            throw new Error('No shipping method was found.');
                        }

                        let selectedShippingMethodName = shippingMethodName;
                        shippingMethods.ShippingMethods.forEach(shippingMethod => {

                            if (shippingMethod.Name === selectedShippingMethodName) {
                                shippingMethods.SelectedShippingProviderId = shippingMethod.ShippingProviderId;
                            }
                        });

                        var vm = {
                            EditMode: this.editShippingMethod,
                            ShippingMethods: shippingMethods,
                            SelectedMethod: selectedShippingMethodName,
                            ShippingMethod: {
                                DisplayName: shippingMethodDisplayName,
                                Cost: shippingMethodCost,
                                Name: shippingMethodName,
                                FulfillmentMethodTypeString: shippingMethodFulfillmentType
                            }
                        };

                        this.render('RecurringCartDetailsShippingMethod', vm);
                    });
            } else {
                var vm = {
                    EditMode: this.editShippingMethod,
                    ShippingMethod: {
                        DisplayName: shippingMethodDisplayName,
                        Cost: shippingMethodCost,
                        Name: shippingMethodName,
                        FulfillmentMethodTypeString: shippingMethodFulfillmentType
                    }
                };
                this.render('RecurringCartDetailsShippingMethod', vm);
            }
        }

        private closeOtherEditSections(actionContext: IControllerActionContext, type: EditSection) {

            if (this.editNextOcurrence && type !== EditSection.NextOccurence) {
                this.toggleEditNextOccurence(actionContext);
            }
            if (this.editShippingMethod && type !== EditSection.ShippingMethod) {
                this.toggleEditShippingMethod(actionContext);
            }
        }

        public getShippingMethods(cartName) : Q.Promise<any> {
            let param: IRecurringOrderGetCartShippingMethods = {
                CartName: cartName
            };
            return this.recurringOrderService.getCartShippingMethods(param)
                    .fail((reason) => {
                        console.error('Error while retrieving shipping methods', reason);
                    });
        }

        public saveEditShippingMethod(actionContext: IControllerActionContext) {
            var context: JQuery = actionContext.elementContext;

            //var newType = context.data('fulfillment-method-type');
            let element = $('#ShippingMethod').find('input[name=ShippingMethod]:checked')[0];

            var newType = element.dataset['fulfillmentMethodType'];


            this.manageSaveShippingMethod(newType);
        }

        public methodSelected(actionContext: IControllerActionContext) {
            var shippingProviderId = actionContext.elementContext.data('shipping-provider-id');
             $('#ShippingProviderId').val(shippingProviderId.toString());
        }

        private manageSaveShippingMethod(newType) {
            //When shipping method is changed from ship to store and ship to home, address must correspond to 
            //store adress/home address.
            //When the type change, we wait to save shipping method and open adresse section. Then, when saving valid address,
            //also save the shipping method.
            //When cancel in one of the two steps, revert to original values.
            //If saving shipping method and the method type doesn't change, save immediatly.


            this.hasShippingMethodTypeChanged = this.originalShippingMethodType !== newType;

            if (this.hasShippingMethodTypeChanged) {
//TODO
            } else {
                //Do the save

                let shippingProviderId = $('#ShippingProviderId').val();

                let element = $('#ShippingMethod').find('input[name=ShippingMethod]:checked');
                let shippingMethodName = element.val();

                //var busy = this.asyncBusy({ elementContext: actionContext.elementContext });

                let cartName = this.context.viewModel.Name;

                let data: IRecurringOrderCartUpdateShippingMethodParam = {
                    shippingProviderId: shippingProviderId,
                    shippingMethodName: shippingMethodName,
                    cartName: cartName
                };

                this.recurringOrderService.updateCartShippingMethod(data)
                    .then(result => {
                        //$("#changeShippingMethodModal").modal('hide');
                        //result.Index = index;
                        //this.customRender("RecurringOrderDetailsLineItemsGroup" + index, "RecurringOrderDetailsLineItemsGroup", result);
                        //This is to reinitialize the popover       
                        //this.initializePopOver();

                        this.reRenderCartPage(result);
                    })
                    .fail((reason) => {
                        console.error(reason);
                    });
                    //.fin(() => busy.done());
            }
        }

        private reRenderCartPage(vm) {
            this.render('MyRecurringCartDetails', vm);
        }
    }
}
